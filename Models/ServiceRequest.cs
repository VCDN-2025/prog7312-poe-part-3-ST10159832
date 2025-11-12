using System.ComponentModel;

namespace ST10159832KeenanPROGpart1.Models
{
    public enum RequestStatus { Open, InProgress, Resolved, Closed }
    public enum RequestPriority { Low = 1, Medium = 2, High = 3, Critical = 4 }

    public class ServiceRequest : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Location { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public RequestPriority Priority { get; set; } = RequestPriority.Medium;

        private RequestStatus _status = RequestStatus.Open;
        public RequestStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                UpdatedAt = DateTime.UtcNow;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            }
        }

        // 🔹 Lecturer feedback: parent → child dependency
        public string ParentId { get; set; }             // ID of the main issue (null if this is a main issue)
        public List<ServiceRequest> Dependents { get; set; } = new(); // children linked to this one

        // 🔹 Used for the heap to determine which job should be done next
        public DateTime EtaUtc { get; set; } = DateTime.UtcNow.AddHours(2);

        public event PropertyChangedEventHandler PropertyChanged;
    }
}