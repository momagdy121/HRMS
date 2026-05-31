using HRSystem.Common.Enums;
using HRSystem.Data.Models;

namespace HRSystem.Business.Helpers;

public static class LeaveWorkflow
{
    public static void Approve(LeaveRequest request, int approvedByEmployeeId)
    {
        request.Status = LeaveRequestStatus.Approved;
        request.ApprovedBy = approvedByEmployeeId;
        request.ApprovedAt = DateTime.UtcNow;
        request.RejectionReason = null;
    }

    public static void Reject(LeaveRequest request, string rejectionReason)
    {
        request.Status = LeaveRequestStatus.Rejected;
        request.RejectionReason = rejectionReason.Trim();
        request.ApprovedBy = null;
        request.ApprovedAt = null;
    }
}
