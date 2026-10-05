namespace Arniston_Letting_Front.Models.Dashboard;

public class DashboardDto
{
    public int TotalBookings { get; set; }

    public int TotalProperties { get; set; }

    public int TotalOwners { get; set; }

    public int TotalCleaners { get; set; }

    public int PendingTasks { get; set; }

    public int OpenBreakages { get; set; }

    public int PendingNotifications { get; set; }
}