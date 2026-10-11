namespace HRSystem.Business.Helpers;

public static class EmailTemplates
{
    public static string PasswordResetEmail(string employeeName, string resetUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Password Reset - HRMS Portal</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #f4f6f9; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;"">
    <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #f4f6f9; padding: 40px 20px;"">
        <tr>
            <td align=""center"">
                <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""600"" style=""max-width: 600px; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 2px 12px rgba(0,0,0,0.08);"">
                    <!-- Header -->
                    <tr>
                        <td style=""background: linear-gradient(135deg, #1a237e 0%, #283593 100%); padding: 32px 40px; text-align: center;"">
                            <h1 style=""color: #ffffff; margin: 0; font-size: 24px; font-weight: 600; letter-spacing: -0.5px;"">HRMS Portal</h1>
                        </td>
                    </tr>
                    <!-- Body -->
                    <tr>
                        <td style=""padding: 40px;"">
                            <h2 style=""color: #1a237e; margin: 0 0 16px; font-size: 20px; font-weight: 600;"">Password Reset Request</h2>
                            <p style=""color: #455a64; font-size: 15px; line-height: 1.6; margin: 0 0 24px;"">
                                Hi {System.Net.WebUtility.HtmlEncode(employeeName)},
                            </p>
                            <p style=""color: #455a64; font-size: 15px; line-height: 1.6; margin: 0 0 32px;"">
                                We received a request to reset the password for your HRMS Portal account. Click the button below to set a new password. This link will expire for security reasons.
                            </p>
                            <!-- CTA Button -->
                            <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                                <tr>
                                    <td align=""center"">
                                        <a href=""{resetUrl}"" style=""display: inline-block; background: linear-gradient(135deg, #1a237e 0%, #283593 100%); color: #ffffff; text-decoration: none; padding: 14px 40px; border-radius: 8px; font-size: 15px; font-weight: 600; letter-spacing: 0.3px;"">
                                            Reset My Password
                                        </a>
                                    </td>
                                </tr>
                            </table>
                            <p style=""color: #78909c; font-size: 13px; line-height: 1.6; margin: 32px 0 0; padding-top: 24px; border-top: 1px solid #eceff1;"">
                                If you didn't request a password reset, you can safely ignore this email. Your password will remain unchanged.
                            </p>
                            <p style=""color: #90a4ae; font-size: 12px; line-height: 1.5; margin: 16px 0 0;"">
                                If the button doesn't work, copy and paste this link into your browser:<br />
                                <a href=""{resetUrl}"" style=""color: #1a237e; word-break: break-all;"">{resetUrl}</a>
                            </p>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style=""background-color: #f8f9fa; padding: 24px 40px; text-align: center; border-top: 1px solid #eceff1;"">
                            <p style=""color: #90a4ae; font-size: 12px; margin: 0;"">
                                &copy; {DateTime.Now.Year} HRMS Portal. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    public static string TaskAssignedEmail(
        string assigneeName,
        string assignerName,
        string taskTitle,
        string? taskDescription,
        DateOnly? dueDate,
        string taskUrl)
    {
        var dueFormatted = dueDate.HasValue ? dueDate.Value.ToString("MMMM dd, yyyy") : "No due date specified";
        var descriptionHtml = string.IsNullOrWhiteSpace(taskDescription)
            ? "<em style=\"color: #94a3b8;\">No description provided.</em>"
            : System.Net.WebUtility.HtmlEncode(taskDescription).Replace("\n", "<br/>");

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>New Task Assigned - HRMS Portal</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #f4f6f9; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;"">
    <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #f4f6f9; padding: 40px 20px;"">
        <tr>
            <td align=""center"">
                <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""600"" style=""max-width: 600px; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 2px 12px rgba(0,0,0,0.08);"">
                    <!-- Header -->
                    <tr>
                        <td style=""background: linear-gradient(135deg, #1f4e79 0%, #00375e 100%); padding: 32px 40px; text-align: left;"">
                            <span style=""display: inline-block; background-color: rgba(255,255,255,0.2); color: #ffffff; font-size: 11px; font-weight: 700; letter-spacing: 1px; text-transform: uppercase; padding: 4px 10px; border-radius: 6px; margin-bottom: 8px;"">Task Assignment</span>
                            <h1 style=""color: #ffffff; margin: 0; font-size: 24px; font-weight: 700; letter-spacing: -0.5px;"">HRMS Portal</h1>
                        </td>
                    </tr>
                    <!-- Body -->
                    <tr>
                        <td style=""padding: 36px 40px;"">
                            <h2 style=""color: #1f4e79; margin: 0 0 16px; font-size: 20px; font-weight: 600;"">You Have Been Assigned a New Task</h2>
                            <p style=""color: #455a64; font-size: 15px; line-height: 1.6; margin: 0 0 16px;"">
                                Hi {System.Net.WebUtility.HtmlEncode(assigneeName)},
                            </p>
                            <p style=""color: #455a64; font-size: 15px; line-height: 1.6; margin: 0 0 24px;"">
                                <strong>{System.Net.WebUtility.HtmlEncode(assignerName)}</strong> has assigned a new task to you with the following details:
                            </p>

                            <!-- Task Details Card -->
                            <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; margin: 0 0 28px;"">
                                <tr>
                                    <td style=""padding: 20px;"">
                                        <div style=""margin-bottom: 14px;"">
                                            <span style=""color: #64748b; font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px;"">Task Title</span>
                                            <h3 style=""color: #0f172a; margin: 4px 0 0; font-size: 17px; font-weight: 700;"">{System.Net.WebUtility.HtmlEncode(taskTitle)}</h3>
                                        </div>

                                        <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""margin-bottom: 14px;"">
                                            <tr>
                                                <td width=""120"" style=""color: #64748b; font-size: 13px; font-weight: 600; padding-bottom: 6px;"">Assigned By:</td>
                                                <td style=""color: #1e293b; font-size: 14px; font-weight: 600; padding-bottom: 6px;"">{System.Net.WebUtility.HtmlEncode(assignerName)}</td>
                                            </tr>
                                            <tr>
                                                <td width=""120"" style=""color: #64748b; font-size: 13px; font-weight: 600; padding-bottom: 6px;"">Due Date:</td>
                                                <td style=""color: {(dueDate.HasValue ? "#0f172a" : "#64748b")}; font-size: 14px; font-weight: 600; padding-bottom: 6px;"">{dueFormatted}</td>
                                            </tr>
                                            <tr>
                                                <td width=""120"" style=""color: #64748b; font-size: 13px; font-weight: 600;"">Status:</td>
                                                <td style=""color: #d97706; font-size: 13px; font-weight: 700;"">Pending</td>
                                            </tr>
                                        </table>

                                        <div>
                                            <span style=""color: #64748b; font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; display: block; margin-bottom: 6px;"">Description</span>
                                            <div style=""color: #334155; font-size: 14px; line-height: 1.6; background-color: #ffffff; border: 1px solid #e2e8f0; border-radius: 6px; padding: 12px;"">
                                                {descriptionHtml}
                                            </div>
                                        </div>
                                    </td>
                                </tr>
                            </table>

                            <!-- CTA Button -->
                            <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""margin-bottom: 24px;"">
                                <tr>
                                    <td align=""center"">
                                        <a href=""{taskUrl}"" style=""display: inline-block; background: linear-gradient(135deg, #1f4e79 0%, #00375e 100%); color: #ffffff; text-decoration: none; padding: 14px 40px; border-radius: 8px; font-size: 15px; font-weight: 600; letter-spacing: 0.3px;"">
                                            View Tasks in Portal →
                                        </a>
                                    </td>
                                </tr>
                            </table>

                            <p style=""color: #78909c; font-size: 13px; line-height: 1.6; margin: 24px 0 0; padding-top: 20px; border-top: 1px solid #eceff1; text-align: center;"">
                                Please log in to your HRMS dashboard to track and update the status of your task (Pending → In Progress → Completed).
                            </p>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style=""background-color: #f8f9fa; padding: 20px 40px; text-align: center; border-top: 1px solid #eceff1;"">
                            <p style=""color: #90a4ae; font-size: 12px; margin: 0;"">
                                &copy; {DateTime.Now.Year} HRMS Portal. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    public static string TaskUpdatedEmail(
        string assigneeName,
        string updaterName,
        string taskTitle,
        string? taskDescription,
        DateOnly? dueDate,
        string taskUrl)
    {
        var dueFormatted = dueDate.HasValue ? dueDate.Value.ToString("MMMM dd, yyyy") : "No due date specified";
        var descriptionHtml = string.IsNullOrWhiteSpace(taskDescription)
            ? "<em style=\"color: #94a3b8;\">No description provided.</em>"
            : System.Net.WebUtility.HtmlEncode(taskDescription).Replace("\n", "<br/>");

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Task Details Updated - HRMS Portal</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #f4f6f9; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;"">
    <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #f4f6f9; padding: 40px 20px;"">
        <tr>
            <td align=""center"">
                <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""600"" style=""max-width: 600px; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 2px 12px rgba(0,0,0,0.08);"">
                    <!-- Header -->
                    <tr>
                        <td style=""background: linear-gradient(135deg, #1f4e79 0%, #00375e 100%); padding: 32px 40px; text-align: left;"">
                            <span style=""display: inline-block; background-color: rgba(255,255,255,0.2); color: #ffffff; font-size: 11px; font-weight: 700; letter-spacing: 1px; text-transform: uppercase; padding: 4px 10px; border-radius: 6px; margin-bottom: 8px;"">Task Update</span>
                            <h1 style=""color: #ffffff; margin: 0; font-size: 24px; font-weight: 700; letter-spacing: -0.5px;"">HRMS Portal</h1>
                        </td>
                    </tr>
                    <!-- Body -->
                    <tr>
                        <td style=""padding: 36px 40px;"">
                            <h2 style=""color: #1f4e79; margin: 0 0 16px; font-size: 20px; font-weight: 600;"">Task Details Have Been Updated</h2>
                            <p style=""color: #455a64; font-size: 15px; line-height: 1.6; margin: 0 0 16px;"">
                                Hi {System.Net.WebUtility.HtmlEncode(assigneeName)},
                            </p>
                            <p style=""color: #455a64; font-size: 15px; line-height: 1.6; margin: 0 0 24px;"">
                                <strong>{System.Net.WebUtility.HtmlEncode(updaterName)}</strong> has updated the details for your task:
                            </p>

                            <!-- Task Details Card -->
                            <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; margin: 0 0 28px;"">
                                <tr>
                                    <td style=""padding: 20px;"">
                                        <div style=""margin-bottom: 14px;"">
                                            <span style=""color: #64748b; font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px;"">Task Title</span>
                                            <h3 style=""color: #0f172a; margin: 4px 0 0; font-size: 17px; font-weight: 700;"">{System.Net.WebUtility.HtmlEncode(taskTitle)}</h3>
                                        </div>

                                        <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""margin-bottom: 14px;"">
                                            <tr>
                                                <td width=""120"" style=""color: #64748b; font-size: 13px; font-weight: 600; padding-bottom: 6px;"">Updated By:</td>
                                                <td style=""color: #1e293b; font-size: 14px; font-weight: 600; padding-bottom: 6px;"">{System.Net.WebUtility.HtmlEncode(updaterName)}</td>
                                            </tr>
                                            <tr>
                                                <td width=""120"" style=""color: #64748b; font-size: 13px; font-weight: 600; padding-bottom: 6px;"">Due Date:</td>
                                                <td style=""color: {(dueDate.HasValue ? "#0f172a" : "#64748b")}; font-size: 14px; font-weight: 600; padding-bottom: 6px;"">{dueFormatted}</td>
                                            </tr>
                                        </table>

                                        <div>
                                            <span style=""color: #64748b; font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; display: block; margin-bottom: 6px;"">Updated Description</span>
                                            <div style=""color: #334155; font-size: 14px; line-height: 1.6; background-color: #ffffff; border: 1px solid #e2e8f0; border-radius: 6px; padding: 12px;"">
                                                {descriptionHtml}
                                            </div>
                                        </div>
                                    </td>
                                </tr>
                            </table>

                            <!-- CTA Button -->
                            <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""margin-bottom: 24px;"">
                                <tr>
                                    <td align=""center"">
                                        <a href=""{taskUrl}"" style=""display: inline-block; background: linear-gradient(135deg, #1f4e79 0%, #00375e 100%); color: #ffffff; text-decoration: none; padding: 14px 40px; border-radius: 8px; font-size: 15px; font-weight: 600; letter-spacing: 0.3px;"">
                                            View Task in Portal →
                                        </a>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style=""background-color: #f8f9fa; padding: 20px 40px; text-align: center; border-top: 1px solid #eceff1;"">
                            <p style=""color: #90a4ae; font-size: 12px; margin: 0;"">
                                &copy; {DateTime.Now.Year} HRMS Portal. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }
}
