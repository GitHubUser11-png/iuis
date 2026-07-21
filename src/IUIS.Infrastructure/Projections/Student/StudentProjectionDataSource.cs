using IUIS.Application.Abstractions.StudentSelfService;
using IUIS.Infrastructure.Persistence;

namespace IUIS.Infrastructure.Projections.Student
{
    public sealed class StudentProjectionDataSource : IStudentProjectionDataSource
    {
        private readonly IJsonFileRepository<Student> _studentRepository;
        private readonly IJsonFileRepository<Course> _courseRepository;
        private readonly IJsonFileRepository<Subject> _subjectRepository;
        private readonly IJsonFileRepository<Enrollment> _enrollmentRepository;
        private readonly IJsonFileRepository<TuitionAssessment> _assessmentRepository;
        private readonly IJsonFileRepository<Payment> _paymentRepository;
        private readonly IJsonFileRepository<ScholarshipAward> _scholarshipRepository;
        private readonly IJsonFileRepository<Book> _bookRepository;
        private readonly IJsonFileRepository<Borrowing> _borrowingRepository;
        private readonly IJsonFileRepository<Notification> _notificationRepository;

        public StudentProjectionDataSource(
            IJsonFileRepository<Student> studentRepository,
            IJsonFileRepository<Course> courseRepository,
            IJsonFileRepository<Subject> subjectRepository,
            IJsonFileRepository<Enrollment> enrollmentRepository,
            IJsonFileRepository<TuitionAssessment> assessmentRepository,
            IJsonFileRepository<Payment> paymentRepository,
            IJsonFileRepository<ScholarshipAward> scholarshipRepository,
            IJsonFileRepository<Book> bookRepository,
            IJsonFileRepository<Borrowing> borrowingRepository,
            IJsonFileRepository<Notification> notificationRepository)
        {
            _studentRepository = studentRepository;
            _courseRepository = courseRepository;
            _subjectRepository = subjectRepository;
            _enrollmentRepository = enrollmentRepository;
            _assessmentRepository = assessmentRepository;
            _paymentRepository = paymentRepository;
            _scholarshipRepository = scholarshipRepository;
            _bookRepository = bookRepository;
            _borrowingRepository = borrowingRepository;
            _notificationRepository = notificationRepository;
        }

        public StudentProjectionSnapshot ReadStudentSources(string studentId)
        {
            var snapshot = new StudentProjectionSnapshot
            {
                CapturedAtUtc = System.DateTime.UtcNow
            };

            var studentEnvelope = _studentRepository.LoadEnvelope();
            snapshot.SourceRevisions = studentEnvelope.RevisionDictionary;
            snapshot.Students = studentEnvelope.Records;

            var courseEnvelope = _courseRepository.LoadEnvelope();
            snapshot.Courses = courseEnvelope.Records;

            var subjectEnvelope = _subjectRepository.LoadEnvelope();
            snapshot.Subjects = subjectEnvelope.Records;

            var enrollmentEnvelope = _enrollmentRepository.LoadEnvelope();
            snapshot.Enrollments = enrollmentEnvelope.Records;

            var assessmentEnvelope = _assessmentRepository.LoadEnvelope();
            snapshot.TuitionAssessments = assessmentEnvelope.Records;

            var paymentEnvelope = _paymentRepository.LoadEnvelope();
            snapshot.Payments = paymentEnvelope.Records;

            var scholarshipEnvelope = _scholarshipRepository.LoadEnvelope();
            snapshot.Scholarships = scholarshipEnvelope.Records;

            var bookEnvelope = _bookRepository.LoadEnvelope();
            snapshot.Books = bookEnvelope.Records;

            var borrowingEnvelope = _borrowingRepository.LoadEnvelope();
            snapshot.Borrowings = borrowingEnvelope.Records;

            var notificationEnvelope = _notificationRepository.LoadEnvelope();
            snapshot.Notifications = notificationEnvelope.Records;

            return snapshot;
        }
    }
}
