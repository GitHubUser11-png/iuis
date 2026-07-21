from pathlib import Path


def replace_once(path, old, new):
    file_path = Path(path)
    text = file_path.read_text(encoding="utf-8")
    count = text.count(old)
    if count != 1:
        raise RuntimeError(
            "Expected exactly one baseline match in {0}; found {1}.".format(
                path,
                count))
    file_path.write_text(text.replace(old, new, 1), encoding="utf-8")


def patch_schema_guard():
    replace_once(
        "src/IUIS.Infrastructure/Persistence/SpecializedAggregateJsonMappers.cs",
        """            if (record.RecordSchemaVersion != 0
                && record.RecordSchemaVersion != CurrentRecordSchemaVersion)""",
        """            if (record.RecordSchemaVersion != CurrentRecordSchemaVersion)""")


def patch_payment_concurrency():
    path = Path(
        "src/IUIS.Application/Orchestration/EnrollmentFinanceCommandServices.cs")
    text = path.read_text(encoding="utf-8")
    old_contract = """    public sealed class PaymentAllocationInput
    {
        public string AssessmentId { get; set; }
        public decimal Amount { get; set; }
    }"""
    new_contract = """    public sealed class PaymentAllocationInput
    {
        public string AssessmentId { get; set; }
        public long ExpectedAssessmentEntityVersion { get; set; }
        public decimal Amount { get; set; }
    }"""
    if text.count(old_contract) != 1:
        raise RuntimeError("PaymentAllocationInput baseline did not match exactly once.")
    text = text.replace(old_contract, new_contract, 1)

    start_marker = (
        "                    if (request.Allocations == null "
        "|| request.Allocations.Count == 0)")
    end_marker = (
        "                    payment.Post(receiptNumber, utcNow, "
        "principal.UserId);")
    start = text.find(start_marker)
    end_start = text.find(end_marker, start)
    if start < 0 or end_start < 0:
        raise RuntimeError("Payment posting validation block was not found.")
    end = end_start + len(end_marker)

    replacement = """                    if (request.Allocations == null || request.Allocations.Count == 0)
                        throw new ArgumentException(
                            "At least one Payment allocation is required.",
                            nameof(request));
                    var allocationInputs = request.Allocations.ToList();
                    if (allocationInputs.Any(item => item == null))
                        throw new ArgumentException(
                            "Payment allocation input is required.",
                            nameof(request));

                    var amount = new Money(request.Amount, request.CurrencyCode);
                    if (allocationInputs.Sum(item => item.Amount) != amount.Amount)
                        throw new InvalidOperationException(
                            "Payment allocations must equal the Payment amount.");

                    var assessmentIds = new HashSet<string>(StringComparer.Ordinal);
                    foreach (var allocationInput in allocationInputs)
                    {
                        var assessment = EnrollmentFinanceCommandGuard.Find(
                            assessmentSnapshot.Records,
                            allocationInput.AssessmentId,
                            "Tuition Assessment");
                        EnrollmentFinanceCommandGuard.RequireVersion(
                            allocationInput.ExpectedAssessmentEntityVersion,
                            assessment.Version,
                            "Tuition Assessment");
                        if (!assessmentIds.Add(assessment.Id))
                            throw new InvalidOperationException(
                                "A Payment can contain only one allocation for each Assessment.");
                        if (allocationInput.Amount <= 0m)
                            throw new InvalidOperationException(
                                "Payment allocation amounts must be greater than zero.");
                        new Money(allocationInput.Amount, request.CurrencyCode);
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
                    }

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
                    foreach (var allocationInput in allocationInputs)
                    {
                        payment.AddAllocation(
                            new PaymentAllocation(
                                _ids.Allocate("PAL", utcNow.Year, principal.UserId),
                                allocationInput.AssessmentId,
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
                    payment.Post(receiptNumber, utcNow, principal.UserId);"""
    path.write_text(text[:start] + replacement + text[end:], encoding="utf-8")


def patch_released_projection():
    path = Path(
        "src/IUIS.Application/Orchestration/StudentFinanceQueryService.cs")
    text = path.read_text(encoding="utf-8")
    replacements = [
        (
            """                .Where(item => StringComparer.Ordinal.Equals(item.StudentId, studentId))
                .OrderByDescending(item => item.UpdatedAtUtc)
                .Select(ToEnrollment)""",
            """                .Where(item => StringComparer.Ordinal.Equals(item.StudentId, studentId)
                    && IsStudentVisible(item))
                .OrderByDescending(item => item.UpdatedAtUtc)
                .Select(ToEnrollment)"""),
        (
            """                .Where(item => StringComparer.Ordinal.Equals(item.StudentId, studentId))
                .OrderByDescending(item => item.UpdatedAtUtc)
                .ToList();
            var paymentRecords""",
            """                .Where(item => StringComparer.Ordinal.Equals(item.StudentId, studentId)
                    && IsStudentVisible(item))
                .OrderByDescending(item => item.UpdatedAtUtc)
                .ToList();
            var paymentRecords"""),
        (
            """                .Where(item => StringComparer.Ordinal.Equals(item.StudentId, studentId))
                .OrderByDescending(item => item.ReceivedAtUtc)
                .ToList();
            var adjustmentRecords""",
            """                .Where(item => StringComparer.Ordinal.Equals(item.StudentId, studentId)
                    && IsStudentVisible(item))
                .OrderByDescending(item => item.ReceivedAtUtc)
                .ToList();
            var adjustmentRecords"""),
        (
            """                .Where(item => StringComparer.Ordinal.Equals(item.StudentId, studentId))
                .OrderByDescending(item => item.UpdatedAtUtc)
                .ToList();
            var scholarshipRecords""",
            """                .Where(item => StringComparer.Ordinal.Equals(item.StudentId, studentId)
                    && IsStudentVisible(item))
                .OrderByDescending(item => item.UpdatedAtUtc)
                .ToList();
            var scholarshipRecords"""),
        (
            """                .Where(item => StringComparer.Ordinal.Equals(item.StudentId, studentId))
                .OrderByDescending(item => item.UpdatedAtUtc)
                .ToList();

            var currencyCode""",
            """                .Where(item => StringComparer.Ordinal.Equals(item.StudentId, studentId)
                    && IsStudentVisible(item))
                .OrderByDescending(item => item.UpdatedAtUtc)
                .ToList();

            var currencyCode""")
    ]
    for old, new in replacements:
        if text.count(old) != 1:
            raise RuntimeError(
                "Student Finance lifecycle baseline did not match exactly once.")
        text = text.replace(old, new, 1)

    marker = "        private static StudentEnrollmentDto ToEnrollment(Enrollment value)"
    if text.count(marker) != 1:
        raise RuntimeError("Student Finance helper insertion point was not found.")
    helpers = """        private static bool IsStudentVisible(Enrollment value)
        {
            return value.Status != EnrollmentStatus.Unspecified
                && value.Status != EnrollmentStatus.Draft;
        }

        private static bool IsStudentVisible(TuitionAssessment value)
        {
            return value.Status == TuitionAssessmentStatus.Posted
                || value.Status == TuitionAssessmentStatus.Cancelled;
        }

        private static bool IsStudentVisible(Payment value)
        {
            return value.Status == PaymentStatus.Posted
                || value.Status == PaymentStatus.Voided;
        }

        private static bool IsStudentVisible(FinancialAdjustment value)
        {
            return value.Status == FinancialAdjustmentStatus.Posted;
        }

        private static bool IsStudentVisible(ScholarshipAward value)
        {
            return value.Status == ScholarshipAwardStatus.Approved
                || value.Status == ScholarshipAwardStatus.Applied
                || value.Status == ScholarshipAwardStatus.Cancelled;
        }

"""
    path.write_text(text.replace(marker, helpers + marker, 1), encoding="utf-8")


def register_tests():
    replace_once(
        "tests/IUIS.Tests/IUIS.Tests.csproj",
        "    <Compile Include=\"Pass11EnvelopeTokenFinanceTests.cs\" />\n",
        "    <Compile Include=\"Pass11EnvelopeTokenFinanceTests.cs\" />\n"
        "    <Compile Include=\"Pass11CorrectiveClosureTests.cs\" />\n")


patch_schema_guard()
patch_payment_concurrency()
patch_released_projection()
register_tests()
