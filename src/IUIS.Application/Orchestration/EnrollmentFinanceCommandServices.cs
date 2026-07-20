using System;
using System.Collections.Generic;
using System.Linq;

using IUIS.Application.Authorization;
using IUIS.Application.Dtos;
using IUIS.Application.Repositories;
using IUIS.Domain.Academic;
using IUIS.Domain.Common;
using IUIS.Domain.Finance;
using IUIS.Domain.Identity;

namespace IUIS.Application.Orchestration
{
    public sealed class EnrollmentSubjectSelection
    {
        public string SubjectId { get; set; }
        public string SubjectCodeSnapshot { get; set; }
        public string SubjectTitleSnapshot { get; set; }
        public decimal UnitsSnapshot { get; set; }
        public int YearLevelSnapshot { get; set; }
        public int TermNumberSnapshot { get; set; }
        public bool IsRequiredSnapshot { get; set; }
        public string SectionCode { get; set; }
    }

    public sealed class EnrollmentSubmissionRequest
    {
        public long ExpectedEnrollmentRepositoryRevision { get; set; }
        public string AcademicPeriodId { get; set; }
        public string CourseIdSnapshot { get; set; }
        public string CourseCodeSnapshot { get; set; }
        public string CourseNameSnapshot { get; set; }
        public string CurriculumIdSnapshot { get; set; }
        public string CurriculumVersionSnapshot { get; set; }
        public IReadOnlyCollection<EnrollmentSubjectSelection> SubjectLines { get; set; }
    }

    public sealed class AssessmentChargeInput
    {
        public string ChargeRuleId { get; set; }
        public string RuleCodeSnapshot { get; set; }
        public string DescriptionSnapshot { get; set; }
        public AssessmentChargeCategory Category { get; set; }
        public decimal Amount { get; set; }
    }

    public sealed class AssessmentPostingRequest
    {
        public long ExpectedEnrollmentRepositoryRevision { get; set; }
        public long ExpectedAssessmentRepositoryRevision { get; set; }
        public long ExpectedEnrollmentEntityVersion { get; set; }
        public string EnrollmentId { get; set; }
        public string CurrencyCode { get; set; }
        public IReadOnlyCollection<AssessmentChargeInput> Charges { get; set; }
    }

    public sealed class PaymentAllocationInput
    {
        public string AssessmentId { get; set; }
        public decimal Amount { get; set; }
    }

    public sealed class PaymentPostingRequest
    {
        public long ExpectedAssessmentRepositoryRevision { get; set; }
        public long ExpectedPaymentRepositoryRevision { get; set; }
        public string StudentId { get; set; }
        public string AcademicPeriodId { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public PaymentMethod Method { get; set; }
        public DateTime ReceivedAtUtc { get; set; }
        public string ExternalReference { get; set; }
        public IReadOnlyCollection<PaymentAllocationInput> Allocations { get; set; }
    }

    public sealed class FinancialAdjustmentPostingRequest
    {
        public long ExpectedAssessmentRepositoryRevision { get; set; }
        public long ExpectedAdjustmentRepositoryRevision { get; set; }
        public long ExpectedAssessmentEntityVersion { get; set; }
        public string AssessmentId { get; set; }
        public FinancialAdjustmentDirection Direction { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public FinancialAdjustmentSourceKind SourceKind { get; set; }
        public string SourceRecordId { get; set; }
        public string Reason { get; set; }
    }

    public sealed class ScholarshipApplicationRequest
    {
        public long ExpectedScholarshipRepositoryRevision { get; set; }
        public long ExpectedAssessmentRepositoryRevision { get; set; }
        public long ExpectedAdjustmentRepositoryRevision { get; set; }
        public long ExpectedScholarshipEntityVersion { get; set; }
        public long ExpectedAssessmentEntityVersion { get; set; }
        public string ScholarshipAwardId { get; set; }
        public string AssessmentId { get; set; }
        public decimal EligibleChargeAmount { get; set; }
    }

    internal static class EnrollmentFinanceCommandGuard
    {
        public static SessionApplicationKind ApplicationKind(
            AuthorizationPrincipal principal)
        {
            return principal.PrimaryRole == PrimaryRole.Administrator
                ? SessionApplicationKind.AdministratorApplication
                : SessionApplicationKind.UserApplication;
        }

        public static void RequireRevision(
            long expected,
            long actual,
            string repositoryName)
        {
            if (expected < 0L)
                throw new ArgumentOutOfRangeException(nameof(expected));
            if (expected != actual)
                throw new InvalidOperationException(
                    "The request is based on a stale "
                    + repositoryName + " repository revision.");
        }

        public static void RequireVersion(
            long expected,
            long actual,
            string entityName)
        {
            if (expected < 1L)
                throw new ArgumentOutOfRangeException(nameof(expected));
            if (expected != actual)
                throw new InvalidOperationException(
                    "The request is based on a stale "
                    + entityName + " entity version.");
        }

        public static T Find<T>(
            IEnumerable<T> records,
            string id,
            string entityName)
            where T : class, IEntity
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(
                    entityName + " ID is required.",
                    nameof(id));
            var value = records.SingleOrDefault(item =>
                StringComparer.Ordinal.Equals(item.Id, id.Trim()));
            if (value == null)
                throw new InvalidOperationException(
                    entityName + " is unavailable.");
            return value;
        }

        public static FinanceCommandResult Result(
            string transactionId,
            EntityBase value,
            long repositoryRevision,
            string secondaryRecordId,
            long secondaryRepositoryRevision,
            string receiptNumber)
        {
            return new FinanceCommandResult
            {
                TransactionId = transactionId,
                RecordId = value.Id,
                SecondaryRecordId = secondaryRecordId,
                ReceiptNumber = receiptNumber,
                RepositoryRevision = repositoryRevision,
                SecondaryRepositoryRevision = secondaryRepositoryRevision,
                EntityVersion = value.Version,
                UpdatedAtUtc = value.UpdatedAtUtc,
                UpdatedByUserId = value.UpdatedByUserId
            };
        }
    }

    public sealed class StudentEnrollmentSubmissionService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly IEnrollmentRepository _enrollments;
        private readonly IApplicationTransactionCoordinator _transactions;
        private readonly IApplicationIdentifierAllocator _ids;

        public StudentEnrollmentSubmissionService(
            SessionAwareRequestExecutor executor,
            IEnrollmentRepository enrollments,
            IApplicationTransactionCoordinator transactions,
            IApplicationIdentifierAllocator ids)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _enrollments = enrollments ?? throw new ArgumentNullException(nameof(enrollments));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _ids = ids ?? throw new ArgumentNullException(nameof(ids));
        }

        public FinanceCommandResult Submit(
            string sessionId,
            string sessionToken,
            EnrollmentSubmissionRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _executor.Command(
                sessionId,
                sessionToken,
                utcNow,
                principal => new AuthorizationRequest(
                    "student.enrollment.submit",
                    SessionApplicationKind.UserApplication,
                    ConfidentialityClassification.OwnRecord,
                    principal.PersonRecordId,
                    new[] { PrimaryRole.Student }),
                principal =>
                {
                    var snapshot = _enrollments.Read();
                    EnrollmentFinanceCommandGuard.RequireRevision(
                        request.ExpectedEnrollmentRepositoryRevision,
                        snapshot.Revision,
                        _enrollments.RepositoryName);
                    if (request.SubjectLines == null || request.SubjectLines.Count == 0)
                        throw new ArgumentException(
                            "At least one Subject selection is required.",
                            nameof(request));
                    if (snapshot.Records.Any(item =>
                        StringComparer.Ordinal.Equals(item.StudentId, principal.PersonRecordId)
                        && StringComparer.Ordinal.Equals(
                            item.AcademicPeriodId,
                            request.AcademicPeriodId)
                        && item.Status != EnrollmentStatus.Rejected
                        && item.Status != EnrollmentStatus.Withdrawn
                        && item.Status != EnrollmentStatus.Cancelled))
                    {
                        throw new InvalidOperationException(
                            "The Student already has an active Enrollment for the Academic Period.");
                    }

                    var enrollment = new Enrollment(
                        _ids.Allocate("ENR", utcNow.Year, principal.UserId),
                        principal.PersonRecordId,
                        request.AcademicPeriodId,
                        request.CourseIdSnapshot,
                        request.CourseCodeSnapshot,
                        request.CourseNameSnapshot,
                        request.CurriculumIdSnapshot,
                        request.CurriculumVersionSnapshot,
                        utcNow,
                        principal.UserId);
                    foreach (var selection in request.SubjectLines)
                    {
                        if (selection == null)
                            throw new ArgumentException(
                                "Enrollment Subject selection is required.",
                                nameof(request));
                        enrollment.AddSubjectLine(
                            new EnrollmentSubjectLine(
                                selection.SubjectId,
                                selection.SubjectCodeSnapshot,
                                selection.SubjectTitleSnapshot,
                                selection.UnitsSnapshot,
                                selection.YearLevelSnapshot,
                                selection.TermNumberSnapshot,
                                selection.IsRequiredSnapshot,
                                selection.SectionCode),
                            utcNow,
                            principal.UserId);
                    }
                    enrollment.Submit(utcNow, principal.UserId);

                    var records = snapshot.Records.ToList();
                    records.Add(enrollment);
                    var transactionId = _transactions.Execute(scope =>
                        scope.Stage(
                            _enrollments,
                            records,
                            snapshot.Revision,
                            principal.UserId));
                    return EnrollmentFinanceCommandGuard.Result(
                        transactionId,
                        enrollment,
                        checked(snapshot.Revision + 1L),
                        null,
                        0L,
                        null);
                });
        }
    }

    public sealed class TuitionAssessmentPostingService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly IEnrollmentRepository _enrollments;
        private readonly ITuitionAssessmentRepository _assessments;
        private readonly IApplicationTransactionCoordinator _transactions;
        private readonly IApplicationIdentifierAllocator _ids;

        public TuitionAssessmentPostingService(
            SessionAwareRequestExecutor executor,
            IEnrollmentRepository enrollments,
            ITuitionAssessmentRepository assessments,
            IApplicationTransactionCoordinator transactions,
            IApplicationIdentifierAllocator ids)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _enrollments = enrollments ?? throw new ArgumentNullException(nameof(enrollments));
            _assessments = assessments ?? throw new ArgumentNullException(nameof(assessments));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _ids = ids ?? throw new ArgumentNullException(nameof(ids));
        }

        public FinanceCommandResult Post(
            string sessionId,
            string sessionToken,
            AssessmentPostingRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _executor.Command(
                sessionId,
                sessionToken,
                utcNow,
                principal => new AuthorizationRequest(
                    "finance.assessment.post",
                    EnrollmentFinanceCommandGuard.ApplicationKind(principal),
                    ConfidentialityClassification.Internal,
                    null,
                    new[]
                    {
                        PrimaryRole.EmployeeFaculty,
                        PrimaryRole.Administrator
                    }),
                principal =>
                {
                    var enrollmentSnapshot = _enrollments.Read();
                    var assessmentSnapshot = _assessments.Read();
                    EnrollmentFinanceCommandGuard.RequireRevision(
                        request.ExpectedEnrollmentRepositoryRevision,
                        enrollmentSnapshot.Revision,
                        _enrollments.RepositoryName);
                    EnrollmentFinanceCommandGuard.RequireRevision(
                        request.ExpectedAssessmentRepositoryRevision,
                        assessmentSnapshot.Revision,
                        _assessments.RepositoryName);
                    var enrollment = EnrollmentFinanceCommandGuard.Find(
                        enrollmentSnapshot.Records,
                        request.EnrollmentId,
                        "Enrollment");
                    EnrollmentFinanceCommandGuard.RequireVersion(
                        request.ExpectedEnrollmentEntityVersion,
                        enrollment.Version,
                        "Enrollment");
                    if (enrollment.Status != EnrollmentStatus.Approved)
                        throw new InvalidOperationException(
                            "Only an Approved Enrollment can be assessed.");
                    if (assessmentSnapshot.Records.Any(item =>
                        StringComparer.Ordinal.Equals(
                            item.EnrollmentId,
                            enrollment.Id)
                        && item.Status == TuitionAssessmentStatus.Posted))
                    {
                        throw new InvalidOperationException(
                            "The Enrollment already has a Posted Tuition Assessment.");
                    }
                    if (request.Charges == null || request.Charges.Count == 0)
                        throw new ArgumentException(
                            "At least one assessment charge is required.",
                            nameof(request));

                    var assessment = new TuitionAssessment(
                        _ids.Allocate("ASM", utcNow.Year, principal.UserId),
                        enrollment.StudentId,
                        enrollment.Id,
                        enrollment.AcademicPeriodId,
                        request.CurrencyCode,
                        utcNow,
                        principal.UserId);
                    foreach (var input in request.Charges)
                    {
                        if (input == null)
                            throw new ArgumentException(
                                "Assessment charge input is required.",
                                nameof(request));
                        assessment.AddChargeLine(
                            new AssessmentChargeLine(
                                _ids.Allocate("ACL", utcNow.Year, principal.UserId),
                                input.ChargeRuleId,
                                input.RuleCodeSnapshot,
                                input.DescriptionSnapshot,
                                input.Category,
                                new Money(input.Amount, request.CurrencyCode)),
                            utcNow,
                            principal.UserId);
                    }
                    assessment.Post(utcNow, principal.UserId);

                    var records = assessmentSnapshot.Records.ToList();
                    records.Add(assessment);
                    var transactionId = _transactions.Execute(scope =>
                        scope.Stage(
                            _assessments,
                            records,
                            assessmentSnapshot.Revision,
                            principal.UserId));
                    return EnrollmentFinanceCommandGuard.Result(
                        transactionId,
                        assessment,
                        checked(assessmentSnapshot.Revision + 1L),
                        enrollment.Id,
                        enrollmentSnapshot.Revision,
                        null);
                });
        }
    }

    public sealed class PaymentPostingService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly ITuitionAssessmentRepository _assessments;
        private readonly IPaymentRepository _payments;
        private readonly IApplicationTransactionCoordinator _transactions;
        private readonly IApplicationIdentifierAllocator _ids;

        public PaymentPostingService(
            SessionAwareRequestExecutor executor,
            ITuitionAssessmentRepository assessments,
            IPaymentRepository payments,
            IApplicationTransactionCoordinator transactions,
            IApplicationIdentifierAllocator ids)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _assessments = assessments ?? throw new ArgumentNullException(nameof(assessments));
            _payments = payments ?? throw new ArgumentNullException(nameof(payments));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _ids = ids ?? throw new ArgumentNullException(nameof(ids));
        }

        public FinanceCommandResult Post(
            string sessionId,
            string sessionToken,
            PaymentPostingRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _executor.Command(
                sessionId,
                sessionToken,
                utcNow,
                principal => new AuthorizationRequest(
                    "finance.payment.post",
                    EnrollmentFinanceCommandGuard.ApplicationKind(principal),
                    ConfidentialityClassification.Internal,
                    null,
                    new[]
                    {
                        PrimaryRole.EmployeeFaculty,
                        PrimaryRole.Administrator
                    }),
                principal =>
                {
                    var assessmentSnapshot = _assessments.Read();
                    var paymentSnapshot = _payments.Read();
                    EnrollmentFinanceCommandGuard.RequireRevision(
                        request.ExpectedAssessmentRepositoryRevision,
                        assessmentSnapshot.Revision,
                        _assessments.RepositoryName);
                    EnrollmentFinanceCommandGuard.RequireRevision(
                        request.ExpectedPaymentRepositoryRevision,
                        paymentSnapshot.Revision,
                        _payments.RepositoryName);
                    if (request.Allocations == null || request.Allocations.Count == 0)
                        throw new ArgumentException(
                            "At least one Payment allocation is required.",
                            nameof(request));
                    var amount = new Money(request.Amount, request.CurrencyCode);
                    if (request.Allocations.Sum(item => item.Amount) != amount.Amount)
                        throw new InvalidOperationException(
                            "Payment allocations must equal the Payment amount.");

                    var payment = new Payment(
                        _ids.Allocate("PAY", utcNow.Year, principal.UserId),
                        request.StudentId,
                        request.AcademicPeriodId,
                        amount,
                        request.Method,
                        request.ReceivedAtUtc,
                        request.ExternalReference,
                        utcNow,
                        principal.UserId);
                    foreach (var allocationInput in request.Allocations)
                    {
                        if (allocationInput == null)
                            throw new ArgumentException(
                                "Payment allocation input is required.",
                                nameof(request));
                        var assessment = EnrollmentFinanceCommandGuard.Find(
                            assessmentSnapshot.Records,
                            allocationInput.AssessmentId,
                            "Tuition Assessment");
                        if (assessment.Status != TuitionAssessmentStatus.Posted
                            || !StringComparer.Ordinal.Equals(
                                assessment.StudentId,
                                request.StudentId)
                            || !StringComparer.Ordinal.Equals(
                                assessment.AcademicPeriodId,
                                request.AcademicPeriodId))
                        {
                            throw new InvalidOperationException(
                                "Each Payment allocation must target a Posted Assessment for the same Student and Academic Period.");
                        }
                        payment.AddAllocation(
                            new PaymentAllocation(
                                _ids.Allocate("PAL", utcNow.Year, principal.UserId),
                                assessment.Id,
                                new Money(
                                    allocationInput.Amount,
                                    request.CurrencyCode)),
                            utcNow,
                            principal.UserId);
                    }

                    var receiptNumber = _ids.Allocate(
                        "RCT",
                        utcNow.Year,
                        principal.UserId);
                    if (paymentSnapshot.Records.Any(item =>
                        StringComparer.Ordinal.Equals(
                            item.ReceiptNumber,
                            receiptNumber)))
                    {
                        throw new InvalidOperationException(
                            "The allocated receipt number already exists.");
                    }
                    payment.Post(receiptNumber, utcNow, principal.UserId);

                    var records = paymentSnapshot.Records.ToList();
                    records.Add(payment);
                    var transactionId = _transactions.Execute(scope =>
                        scope.Stage(
                            _payments,
                            records,
                            paymentSnapshot.Revision,
                            principal.UserId));
                    return EnrollmentFinanceCommandGuard.Result(
                        transactionId,
                        payment,
                        checked(paymentSnapshot.Revision + 1L),
                        null,
                        assessmentSnapshot.Revision,
                        receiptNumber);
                });
        }
    }

    public sealed class FinancialAdjustmentPostingService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly ITuitionAssessmentRepository _assessments;
        private readonly IFinancialAdjustmentRepository _adjustments;
        private readonly IApplicationTransactionCoordinator _transactions;
        private readonly IApplicationIdentifierAllocator _ids;

        public FinancialAdjustmentPostingService(
            SessionAwareRequestExecutor executor,
            ITuitionAssessmentRepository assessments,
            IFinancialAdjustmentRepository adjustments,
            IApplicationTransactionCoordinator transactions,
            IApplicationIdentifierAllocator ids)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _assessments = assessments ?? throw new ArgumentNullException(nameof(assessments));
            _adjustments = adjustments ?? throw new ArgumentNullException(nameof(adjustments));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _ids = ids ?? throw new ArgumentNullException(nameof(ids));
        }

        public FinanceCommandResult Post(
            string sessionId,
            string sessionToken,
            FinancialAdjustmentPostingRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _executor.Command(
                sessionId,
                sessionToken,
                utcNow,
                principal => new AuthorizationRequest(
                    "finance.adjustment.post",
                    EnrollmentFinanceCommandGuard.ApplicationKind(principal),
                    ConfidentialityClassification.Restricted,
                    null,
                    new[]
                    {
                        PrimaryRole.EmployeeFaculty,
                        PrimaryRole.Administrator
                    }),
                principal =>
                {
                    var assessmentSnapshot = _assessments.Read();
                    var adjustmentSnapshot = _adjustments.Read();
                    EnrollmentFinanceCommandGuard.RequireRevision(
                        request.ExpectedAssessmentRepositoryRevision,
                        assessmentSnapshot.Revision,
                        _assessments.RepositoryName);
                    EnrollmentFinanceCommandGuard.RequireRevision(
                        request.ExpectedAdjustmentRepositoryRevision,
                        adjustmentSnapshot.Revision,
                        _adjustments.RepositoryName);
                    var assessment = EnrollmentFinanceCommandGuard.Find(
                        assessmentSnapshot.Records,
                        request.AssessmentId,
                        "Tuition Assessment");
                    EnrollmentFinanceCommandGuard.RequireVersion(
                        request.ExpectedAssessmentEntityVersion,
                        assessment.Version,
                        "Tuition Assessment");
                    if (assessment.Status != TuitionAssessmentStatus.Posted)
                        throw new InvalidOperationException(
                            "Financial Adjustments require a Posted Tuition Assessment.");

                    var adjustment = new FinancialAdjustment(
                        _ids.Allocate("FAD", utcNow.Year, principal.UserId),
                        assessment.StudentId,
                        assessment.Id,
                        request.Direction,
                        new Money(request.Amount, request.CurrencyCode),
                        request.SourceKind,
                        request.SourceRecordId,
                        request.Reason,
                        utcNow,
                        principal.UserId);
                    adjustment.Post(utcNow, principal.UserId);

                    var records = adjustmentSnapshot.Records.ToList();
                    records.Add(adjustment);
                    var transactionId = _transactions.Execute(scope =>
                        scope.Stage(
                            _adjustments,
                            records,
                            adjustmentSnapshot.Revision,
                            principal.UserId));
                    return EnrollmentFinanceCommandGuard.Result(
                        transactionId,
                        adjustment,
                        checked(adjustmentSnapshot.Revision + 1L),
                        assessment.Id,
                        assessmentSnapshot.Revision,
                        null);
                });
        }
    }

    public sealed class ScholarshipAwardApplicationService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly IScholarshipAwardRepository _scholarships;
        private readonly ITuitionAssessmentRepository _assessments;
        private readonly IFinancialAdjustmentRepository _adjustments;
        private readonly IApplicationTransactionCoordinator _transactions;
        private readonly IApplicationIdentifierAllocator _ids;

        public ScholarshipAwardApplicationService(
            SessionAwareRequestExecutor executor,
            IScholarshipAwardRepository scholarships,
            ITuitionAssessmentRepository assessments,
            IFinancialAdjustmentRepository adjustments,
            IApplicationTransactionCoordinator transactions,
            IApplicationIdentifierAllocator ids)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _scholarships = scholarships ?? throw new ArgumentNullException(nameof(scholarships));
            _assessments = assessments ?? throw new ArgumentNullException(nameof(assessments));
            _adjustments = adjustments ?? throw new ArgumentNullException(nameof(adjustments));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _ids = ids ?? throw new ArgumentNullException(nameof(ids));
        }

        public FinanceCommandResult Apply(
            string sessionId,
            string sessionToken,
            ScholarshipApplicationRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _executor.Command(
                sessionId,
                sessionToken,
                utcNow,
                principal => new AuthorizationRequest(
                    "finance.scholarship.apply",
                    EnrollmentFinanceCommandGuard.ApplicationKind(principal),
                    ConfidentialityClassification.Restricted,
                    null,
                    new[]
                    {
                        PrimaryRole.EmployeeFaculty,
                        PrimaryRole.Administrator
                    }),
                principal =>
                {
                    var scholarshipSnapshot = _scholarships.Read();
                    var assessmentSnapshot = _assessments.Read();
                    var adjustmentSnapshot = _adjustments.Read();
                    EnrollmentFinanceCommandGuard.RequireRevision(
                        request.ExpectedScholarshipRepositoryRevision,
                        scholarshipSnapshot.Revision,
                        _scholarships.RepositoryName);
                    EnrollmentFinanceCommandGuard.RequireRevision(
                        request.ExpectedAssessmentRepositoryRevision,
                        assessmentSnapshot.Revision,
                        _assessments.RepositoryName);
                    EnrollmentFinanceCommandGuard.RequireRevision(
                        request.ExpectedAdjustmentRepositoryRevision,
                        adjustmentSnapshot.Revision,
                        _adjustments.RepositoryName);

                    var award = EnrollmentFinanceCommandGuard.Find(
                        scholarshipSnapshot.Records,
                        request.ScholarshipAwardId,
                        "Scholarship Award");
                    var assessment = EnrollmentFinanceCommandGuard.Find(
                        assessmentSnapshot.Records,
                        request.AssessmentId,
                        "Tuition Assessment");
                    EnrollmentFinanceCommandGuard.RequireVersion(
                        request.ExpectedScholarshipEntityVersion,
                        award.Version,
                        "Scholarship Award");
                    EnrollmentFinanceCommandGuard.RequireVersion(
                        request.ExpectedAssessmentEntityVersion,
                        assessment.Version,
                        "Tuition Assessment");
                    if (assessment.Status != TuitionAssessmentStatus.Posted)
                        throw new InvalidOperationException(
                            "Scholarship application requires a Posted Tuition Assessment.");
                    if (!StringComparer.Ordinal.Equals(award.StudentId, assessment.StudentId)
                        || !StringComparer.Ordinal.Equals(
                            award.AcademicPeriodId,
                            assessment.AcademicPeriodId))
                    {
                        throw new InvalidOperationException(
                            "Scholarship Award and Tuition Assessment must belong to the same Student and Academic Period.");
                    }

                    var effect = award.PreviewEffect(
                        assessment.Id,
                        new Money(
                            request.EligibleChargeAmount,
                            assessment.CurrencyCode));
                    var adjustment = FinancialAdjustment.CreateScholarshipCredit(
                        _ids.Allocate("FAD", utcNow.Year, principal.UserId),
                        effect,
                        utcNow,
                        principal.UserId);
                    adjustment.Post(utcNow, principal.UserId);
                    award.MarkApplied(
                        assessment.Id,
                        adjustment.Id,
                        utcNow,
                        principal.UserId);

                    var scholarshipRecords = scholarshipSnapshot.Records.ToList();
                    var adjustmentRecords = adjustmentSnapshot.Records.ToList();
                    adjustmentRecords.Add(adjustment);
                    var transactionId = _transactions.Execute(scope =>
                    {
                        scope.Stage(
                            _scholarships,
                            scholarshipRecords,
                            scholarshipSnapshot.Revision,
                            principal.UserId);
                        scope.Stage(
                            _adjustments,
                            adjustmentRecords,
                            adjustmentSnapshot.Revision,
                            principal.UserId);
                    });
                    return EnrollmentFinanceCommandGuard.Result(
                        transactionId,
                        award,
                        checked(scholarshipSnapshot.Revision + 1L),
                        adjustment.Id,
                        checked(adjustmentSnapshot.Revision + 1L),
                        null);
                });
        }
    }
}
