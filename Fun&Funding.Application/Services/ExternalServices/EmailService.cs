using FluentEmail.Core;
using Fun_Funding.Application.Interfaces.IExternalServices;
using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.Services.ExternalServices
{
    public class EmailService : IEmailService
    {
        private readonly IFluentEmail _fluentEmail;

        public EmailService(IFluentEmail fluentEmail)
        {
            _fluentEmail = fluentEmail;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string content, EmailType type)
        {
            string body = string.Empty;
            switch (type)
            {
                case EmailType.ResetPassword:
                    body = GenerateResetPasswordEmailBody(content);
                    break;
                case EmailType.Register:
                    body = GenerateRegisterEmailBody(content);
                    break;
                           
                default:
                    body = GenerateResetPasswordEmailBody(content);
                    break;
            }
            await _fluentEmail
                .To(toEmail)
                .Subject(subject)
                .Body(body, isHtml: true)
                .SendAsync();
        }
        public async Task SendReportAsync(string toEmail, string projectName, string userName, DateTime reportedDate, string content, List<string> reason)
        {
            string body = string.Empty;
            await _fluentEmail
                .To(toEmail)
                .Subject($@"[WARNNG] - PROJECT: {projectName} HAS BEEN REPORTED")
                .Body(GenerateReportedEmailBody(projectName, userName, reportedDate, content, reason), isHtml: true)
                .SendAsync();
        }
        public async Task SendUserReportAsync(string toEmail, string userName, DateTime reportedDate, string content, List<string> reason)
        {
            string body = string.Empty;
            await _fluentEmail
                .To(toEmail)
                .Subject($@"[WARNNG] - VIOLENT  : {userName} HAS BEEN REPORTED")
                .Body(GenerateUserReportedEmailBody( userName, reportedDate, content, reason), isHtml: true)
                .SendAsync();
        }

        private string GenerateResetPasswordEmailBody(string content)
        {
            return $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Password Reset Email</title>
                    <link href='https://fonts.googleapis.com/css2?family=Poppins:wght@400;600&display=swap' rel='stylesheet'>
                    <style>
                        body {{ 
                            font-family: 'Poppins', sans-serif; 
                            background-color: #EAEAEA; 
                            color: #2F3645; 
                            margin: 0; 
                            padding: 20px 0;
                        }}
                        table {{
                            width: 100%;
                            height: 100%;
                            background-color: #EAEAEA;
                            text-align: center;
                        }}
                        .container {{
                            max-width: 450px;
                            background-color: #FFFFFF;
                            border-radius: 10px;
                            padding: 20px;
                            border: 1px solid #FFFFFF;
                            margin: 30px auto;
                            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                        }}
                        h1 {{ 
                            color: #2F3645; 
                            font-size: 18px;
                            margin-bottom: 15px;
                        }}
                        p {{ 
                            line-height: 1.4;
                            font-size: 14px;
                            color: #2F3645;
                            margin-left: 30px;
                            margin-right: 30px;
                        }}
                        .btn {{ 
                            display: inline-block; 
                            padding: 10px 20px;
                            font-size: 14px;
                            background-color: #1BAA64; 
                            color: #F5F7F8 !important; 
                            text-decoration: none; 
                            border-radius: 5px; 
                            margin-top: 25px;
                            width: 70%;
                        }}
                        footer {{ 
                            margin-top: 30px;
                            font-size: 12px;
                            color: #777; 
                        }}
                    </style>
                </head>
                <body>
                    <table>
                        <tr>
                            <td>
                                <div class='container'>
                                    <img src='https://i.ibb.co/SxKvYLH/Frame-155.png' alt='Fun&Funding Logo' width='200px' style='margin-bottom: 20px; margin-top: 10px' /> <!-- Smaller logo -->
                                    <h1>Your password reset link is ready!</h1>
                                    <p>The password is best not to use anything with Fun&Funding or you and your family, pick something unique!</p>
                                    <a href='{content}' class='btn'>Reset Password</a>
                                    <footer>
                                        <p>Thanks,<br>The Fun&Funding Team</p>
                                        <p>&copy; 2024 Fun&Funding</p>
                                    </footer>
                                </div>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";
        }
        private string GenerateRegisterEmailBody(string content)
        {
            return $@"
        <!DOCTYPE html>
        <html lang='en'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>OTP Verification Email</title>
            <link href='https://fonts.googleapis.com/css2?family=Poppins:wght@400;600&display=swap' rel='stylesheet'>
            <style>
                body {{ 
                    font-family: 'Poppins', sans-serif; 
                    background-color: #EAEAEA; 
                    color: #2F3645; 
                    margin: 0; 
                    padding: 20px 0;
                }}
                table {{
                    width: 100%;
                    height: 100%;
                    background-color: #EAEAEA;
                    text-align: center;
                }}
                .container {{
                    max-width: 450px;
                    background-color: #FFFFFF;
                    border-radius: 10px;
                    padding: 20px;
                    border: 1px solid #FFFFFF;
                    margin: 30px auto;
                    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                }}
                h1 {{ 
                    color: #2F3645; 
                    font-size: 18px;
                    margin-bottom: 15px;
                }}
                p {{ 
                    line-height: 1.4;
                    font-size: 14px;
                    color: #2F3645;
                    margin-left: 30px;
                    margin-right: 30px;
                }}
                .otp-code {{ 
                    display: inline-block; 
                    padding: 10px 20px;
                    font-size: 20px;
                    font-weight: 600;
                    background-color: #1BAA64; 
                    color: #F5F7F8; 
                    text-decoration: none; 
                    border-radius: 5px; 
                    margin-top: 25px;
                    width: auto;
                }}
                footer {{ 
                    margin-top: 30px;
                    font-size: 12px;
                    color: #777; 
                }}
            </style>
        </head>
        <body>
            <table>
                <tr>
                    <td>
                        <div class='container'>
                            <img src='https://i.ibb.co/SxKvYLH/Frame-155.png' alt='Fun&Funding Logo' width='200px' style='margin-bottom: 20px; margin-top: 10px' />
                            <h1>Verify Your Account with the OTP Code</h1>
                            <p>Use the following OTP code to complete your registration and verify your email address. The code is valid for a limited time, so make sure to use it soon.</p>
                            <div class='otp-code'>{content}</div>
                            <p>If you didn't request this code, you can safely ignore this email.</p>
                            <footer>
                                <p>Thanks,<br>The Fun&Funding Team</p>
                                <p>&copy; 2024 Fun&Funding</p>
                            </footer>
                        </div>
                    </td>
                </tr>
            </table>
        </body>
        </html>";
        }
        private string GenerateReportedEmailBody(string projectName, string userName, DateTime reportedDate, string content, List<string> reason)
        {
            return $@"
                <!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Project Report Notification</title>
    <link href=""https://fonts.googleapis.com/css2?family=Poppins:wght@400;600&display=swap"" rel=""stylesheet"">
    <style>
        body {{ 
            font-family: 'Poppins', sans-serif; 
            background-color: #EAEAEA; 
            color: #2F3645; 
            margin: 0; 
            padding: 20px 0;
        }}
        table {{
            width: 100%;
            height: 100%;
            background-color: #EAEAEA;
            text-align: left; /* Change to left */
            padding: 0 20px;
        }}
        .container {{
            background-color: #FFFFFF;
            border-radius: 10px;
            padding: 30px;
            border: 1px solid #FFFFFF;
            margin: 30px auto;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
        }}
        h1, h2 {{
            color: #1BAA64;
            font-size: 24px;
            margin-bottom: 15px;
        }}
        p {{ 
            text-align: left;
            line-height: 1.6;
            font-size: 16px;
            color: #333;
            margin: 10px 0;
        }}
        ul {{
            text-align: left;
            margin: 10px 0 20px 20px;
        }}
        .btn {{ 
            display: inline-block; 
            padding: 10px 20px;
            font-size: 14px;
            background-color: #1BAA64; 
            color: #F5F7F8 !important; 
            text-decoration: none; 
            border-radius: 5px; 
            margin-top: 25px;
            width: 70%;
            transition: background-color 0.3s;
        }}
        .btn:hover {{
            background-color: #17A359;
        }}
        footer {{ 
            margin-top: 30px;
            font-size: 12px;
            color: #777; 
        }}
        table.report-summary {{
            width: 100%;
            margin-top: 20px;
            border-collapse: collapse;
            font-size: 16px;
            color: #555;
        }}
        table.report-summary th, table.report-summary td {{
            padding: 10px;
            border: 1px solid #ddd;
        }}
        table.report-summary th {{
            background-color: #f0f0f0;
            text-align: left;
        }}
    </style>
</head>
<body>
    <table cellpadding=""0"" cellspacing=""0"">
        <tr>
            <td align=""center"">
                <div class=""container"">
                    <!-- Logo Section -->
                    <img src=""https://i.ibb.co/SxKvYLH/Frame-155.png"" alt=""Fun&Funding Logo"" width=""150px"" style=""margin-bottom: 20px;"" />
                    
                    <!-- Heading Section -->
                    <h1>Important: Your Project Has Been Reported</h1>
                    
                    <!-- Message Section -->
                    <p>Dear <strong>{userName}</strong>,</p>
                    <p>We hope this message finds you well. We regret to inform you that your project, <strong>[{projectName}]</strong>, has been reported by a user. It’s crucial that we address these concerns promptly to maintain the integrity of our community.</p>
                    
                    <p>Below may be the reasons for the report:</p>
                    <ul>
                        <li><strong>Inappropriate Content:</strong> To maintain a safe and respectful environment, it's crucial to implement robust content moderation systems. These systems should utilize both automated algorithms and human reviewers to identify and flag harmful or inappropriate material, such as hate speech, graphic violence, or explicit content. The platform should have clear guidelines on what constitutes inappropriate content, and a transparent process for removing such material and notifying the users involved.</li>
    
                        <li><strong>Intellectual Property Violation:</strong> To protect the rights of creators and ensure compliance with copyright laws, it's essential to require proof of licensing and proper attribution for any copyrighted material used in projects. This could involve users submitting documentation or licenses to verify their right to use specific content, as well as implementing an education program on the importance of intellectual property rights for all users.</li>
    
                        <li><strong>Fraudulent Activity:</strong> To foster trust and integrity on the platform, all project information should undergo thorough verification processes. This includes checking for red flags such as inconsistencies in project descriptions, unrealistic funding goals, or lack of transparency from creators. A dedicated team should be tasked with investigating suspicious activities, and users should be encouraged to report any potentially fraudulent behavior they observe.</li>
    
                        <li><strong>Harassment or Abuse:</strong> Monitoring interactions between users is vital for creating a supportive community. This includes establishing reporting mechanisms that allow users to flag abusive behavior or harassment easily. A clear protocol should be in place for reviewing reports, taking appropriate actions against offenders, and providing support to affected users, such as access to counseling resources or guidance on how to handle such situations.</li>
    
                        <li><strong>Misuse of Funds:</strong> To ensure transparency and accountability in funding usage, platforms should implement tracking mechanisms for how funds are allocated and spent. Creators should be required to provide regular updates and reports on their project's financial status and fund utilization. This not only fosters trust among backers but also helps creators stay accountable for their spending decisions.</li>
    
                        <li><strong>Violating Platform Guidelines:</strong> Enforcing compliance with platform guidelines is essential for maintaining order and fairness. Regular reviews of projects and user interactions should be conducted, with a system in place for issuing warnings or taking corrective action against users who violate these guidelines. This process should be transparent and include opportunities for users to appeal decisions if they believe they were wrongly penalized.</li>
    
                        <li><strong>Spam or Fake Project:</strong> The proliferation of spam or fraudulent projects can undermine the integrity of the platform. To combat this, sophisticated algorithms should be deployed to identify spammy behaviors, such as repetitive posting or suspicious account activity. User reports should also be encouraged and taken seriously. Projects flagged as potential spam should undergo thorough reviews before being allowed to remain active on the platform.</li>
    
                        <li><strong>Harmful or Dangerous Product:</strong> To ensure the safety of users, projects must be screened for compliance with safety regulations and standards. This includes assessing whether products meet industry safety requirements and are fit for consumer use. A comprehensive evaluation process should be established, possibly involving third-party assessments or certifications, to validate the safety of proposed projects.</li>
    
                        <li><strong>Hate Speech or Discrimination:</strong> A zero-tolerance policy towards hate speech and discrimination is essential for fostering an inclusive community. The platform should have strict guidelines in place for identifying and removing projects or comments that promote hate or discrimination. Ongoing training and awareness programs can also be implemented to educate users about the impact of such language and behavior, encouraging a more positive environment.</li>
    
                        <li><strong>False Claims or Misrepresentation:</strong> To protect users from deception, it's important to require creators to provide evidence for any claims made about their projects. This may include product prototypes, testimonials, or verifiable statistics. The platform should have a mechanism for users to challenge or report false claims, and a dedicated team should be responsible for investigating these reports and taking appropriate action.</li>
    
                        <li><strong>Inappropriate or Misleading Rewards:</strong> To maintain credibility, rewards offered by creators should be reviewed to ensure they are appropriate and realistically deliverable. A system should be implemented for evaluating reward structures, including feasibility assessments, and creators should be guided on best practices for reward offerings. Misleading or inappropriate rewards should be flagged and addressed promptly.</li>
    
                        <li><strong>Unauthorized Use of Personal Information:</strong> Protecting user privacy is paramount. Strict privacy policies must be enforced, and user consent should be obtained before any personal information is collected or shared. The platform should provide users with clear information on how their data will be used and stored, and mechanisms should be in place to allow users to manage their privacy settings effectively.</li>
    
                        <li><strong>Privacy Concerns:</strong> Guidelines for handling personal information responsibly should be established and communicated to all users. This includes best practices for data protection, as well as clear procedures for reporting privacy concerns. Regular audits of data handling practices should be conducted to ensure compliance with privacy standards and regulations.</li>
    
                        <li><strong>Offensive Language or Imagery:</strong> Establishing and enforcing community standards regarding offensive language and imagery is crucial for creating a respectful atmosphere. A clear policy should outline acceptable behavior and the consequences of violating these standards. Continuous monitoring of content, coupled with user feedback mechanisms, will help maintain a healthy community environment.</li>
    
                        <li><strong>Violating Legal Requirements:</strong> The platform should stay updated on relevant laws and regulations, ensuring that all projects comply with legal requirements. This may involve consulting legal experts or regulatory bodies to guide creators and users. Regular training sessions on legal compliance can also be beneficial, helping users understand their obligations and the potential repercussions of non-compliance.</li>
                    </ul>

                    <p>Please take appropriate action to resolve these issues. If you believe this report was made in error, you can contact our support team for further assistance. We value your contributions and want to ensure that your project continues to thrive.</p>
                    
                    <!-- Report Summary Section -->
                    <h2>Report Summary:</h2>
                    <table class=""report-summary"">
                        <tr>
                            <th>Violator Name:</th>
                            <td>{projectName}</td>
                        </tr>
                         <tr>
                            <th>Reason:</th>
                            <td>{reason}</td>
                        </tr>
                        <tr>
                            <th>Report Date:</th>
                            <td>{reportedDate}</td>
                        </tr>
                        <tr>
                            <th>Description:</th>
                            <td>{content}</td>
                        </tr>
                        
                        
                    </table>
                    
                    <!-- Conclusion Section -->
                    <p>We recommend addressing the issue as soon as possible to avoid any impact on your project status. Our team is here to help you navigate this process smoothly.</p>
                    <p>If you have any questions or require further assistance, feel free to reach out to our support team by clicking the button below:</p>
                    <a href=""mailto:fundandfunding2024@gmail.com"" class=""btn"">Contact Support</a>
                    
                    <!-- Footer Section -->
                    <footer>
                        <span>Thank you for your attention to this matter.<br>The Fun&Funding Team</span>
                        <span>&copy; 2024 Fun&Funding. All rights reserved.</span>
                    </footer>
                </div>
            </td>
        </tr>
    </table>
</body>
</html>


";
        }
        private string GenerateUserReportedEmailBody(string userName, DateTime reportedDate, string content, List<string> reason)
        {
            string reasonHtml = string.Join("", reason.Select(r => $"<li>{r}</li>"));
            return $@"
                <!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Project Report Notification</title>
    <link href=""https://fonts.googleapis.com/css2?family=Poppins:wght@400;600&display=swap"" rel=""stylesheet"">
    <style>
        body {{ 
            font-family: 'Poppins', sans-serif; 
            background-color: #EAEAEA; 
            color: #2F3645; 
            margin: 0; 
            padding: 20px 0;
        }}
        table {{
            width: 100%;
            height: 100%;
            background-color: #EAEAEA;
            text-align: left; /* Change to left */
            padding: 0 20px;
        }}
        .container {{
            background-color: #FFFFFF;
            border-radius: 10px;
            padding: 30px;
            border: 1px solid #FFFFFF;
            margin: 30px auto;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
        }}
        h1, h2 {{
            color: #1BAA64;
            font-size: 24px;
            margin-bottom: 15px;
        }}
        p {{ 
            text-align: left;
            line-height: 1.6;
            font-size: 16px;
            color: #333;
            margin: 10px 0;
        }}
        ul {{
            text-align: left;
            margin: 10px 0 20px 20px;
        }}
        .btn {{ 
            display: inline-block; 
            padding: 10px 20px;
            font-size: 14px;
            background-color: #1BAA64; 
            color: #F5F7F8 !important; 
            text-decoration: none; 
            border-radius: 5px; 
            margin-top: 25px;
            width: 70%;
            transition: background-color 0.3s;
        }}
        .btn:hover {{
            background-color: #17A359;
        }}
        footer {{ 
            margin-top: 30px;
            font-size: 12px;
            color: #777; 
        }}
        table.report-summary {{
            width: 100%;
            margin-top: 20px;
            border-collapse: collapse;
            font-size: 16px;
            color: #555;
        }}
        table.report-summary th, table.report-summary td {{
            padding: 10px;
            border: 1px solid #ddd;
        }}
        table.report-summary th {{
            background-color: #f0f0f0;
            text-align: left;
        }}
    </style>
</head>
<body>
    <table cellpadding=""0"" cellspacing=""0"">
        <tr>
            <td align=""center"">
                <div class=""container"">
                    <!-- Logo Section -->
                    <img src=""https://i.ibb.co/SxKvYLH/Frame-155.png"" alt=""Fun&Funding Logo"" width=""150px"" style=""margin-bottom: 20px;"" />
                    
                    <!-- Heading Section -->
                    <h1>Important: You Has Been Reported</h1>
                    
                    <!-- Message Section -->
                    <p>Dear <strong>{userName}</strong>,</p>
                    <p>We hope this message finds you well. We regret to inform you that due to some action that you has been reported by a user. It’s crucial that we address these concerns promptly to maintain the integrity of our community.</p>
                    
                    <p>Below may be the reasons for the report:</p>
                    <ul>
                        <li><strong>Inappropriate Content:</strong> To maintain a safe and respectful environment, it's crucial to implement robust content moderation systems. These systems should utilize both automated algorithms and human reviewers to identify and flag harmful or inappropriate material, such as hate speech, graphic violence, or explicit content. The platform should have clear guidelines on what constitutes inappropriate content, and a transparent process for removing such material and notifying the users involved.</li>
    
                        <li><strong>Intellectual Property Violation:</strong> To protect the rights of creators and ensure compliance with copyright laws, it's essential to require proof of licensing and proper attribution for any copyrighted material used in projects. This could involve users submitting documentation or licenses to verify their right to use specific content, as well as implementing an education program on the importance of intellectual property rights for all users.</li>
    
                        <li><strong>Fraudulent Activity:</strong> To foster trust and integrity on the platform, all project information should undergo thorough verification processes. This includes checking for red flags such as inconsistencies in project descriptions, unrealistic funding goals, or lack of transparency from creators. A dedicated team should be tasked with investigating suspicious activities, and users should be encouraged to report any potentially fraudulent behavior they observe.</li>
    
                        <li><strong>Harassment or Abuse:</strong> Monitoring interactions between users is vital for creating a supportive community. This includes establishing reporting mechanisms that allow users to flag abusive behavior or harassment easily. A clear protocol should be in place for reviewing reports, taking appropriate actions against offenders, and providing support to affected users, such as access to counseling resources or guidance on how to handle such situations.</li>
    
                        <li><strong>Misuse of Funds:</strong> To ensure transparency and accountability in funding usage, platforms should implement tracking mechanisms for how funds are allocated and spent. Creators should be required to provide regular updates and reports on their project's financial status and fund utilization. This not only fosters trust among backers but also helps creators stay accountable for their spending decisions.</li>
    
                        <li><strong>Violating Platform Guidelines:</strong> Enforcing compliance with platform guidelines is essential for maintaining order and fairness. Regular reviews of projects and user interactions should be conducted, with a system in place for issuing warnings or taking corrective action against users who violate these guidelines. This process should be transparent and include opportunities for users to appeal decisions if they believe they were wrongly penalized.</li>
    
                        <li><strong>Spam or Fake Project:</strong> The proliferation of spam or fraudulent projects can undermine the integrity of the platform. To combat this, sophisticated algorithms should be deployed to identify spammy behaviors, such as repetitive posting or suspicious account activity. User reports should also be encouraged and taken seriously. Projects flagged as potential spam should undergo thorough reviews before being allowed to remain active on the platform.</li>
    
                        <li><strong>Harmful or Dangerous Product:</strong> To ensure the safety of users, projects must be screened for compliance with safety regulations and standards. This includes assessing whether products meet industry safety requirements and are fit for consumer use. A comprehensive evaluation process should be established, possibly involving third-party assessments or certifications, to validate the safety of proposed projects.</li>
    
                        <li><strong>Hate Speech or Discrimination:</strong> A zero-tolerance policy towards hate speech and discrimination is essential for fostering an inclusive community. The platform should have strict guidelines in place for identifying and removing projects or comments that promote hate or discrimination. Ongoing training and awareness programs can also be implemented to educate users about the impact of such language and behavior, encouraging a more positive environment.</li>
    
                        <li><strong>False Claims or Misrepresentation:</strong> To protect users from deception, it's important to require creators to provide evidence for any claims made about their projects. This may include product prototypes, testimonials, or verifiable statistics. The platform should have a mechanism for users to challenge or report false claims, and a dedicated team should be responsible for investigating these reports and taking appropriate action.</li>
    
                        <li><strong>Inappropriate or Misleading Rewards:</strong> To maintain credibility, rewards offered by creators should be reviewed to ensure they are appropriate and realistically deliverable. A system should be implemented for evaluating reward structures, including feasibility assessments, and creators should be guided on best practices for reward offerings. Misleading or inappropriate rewards should be flagged and addressed promptly.</li>
    
                        <li><strong>Unauthorized Use of Personal Information:</strong> Protecting user privacy is paramount. Strict privacy policies must be enforced, and user consent should be obtained before any personal information is collected or shared. The platform should provide users with clear information on how their data will be used and stored, and mechanisms should be in place to allow users to manage their privacy settings effectively.</li>
    
                        <li><strong>Privacy Concerns:</strong> Guidelines for handling personal information responsibly should be established and communicated to all users. This includes best practices for data protection, as well as clear procedures for reporting privacy concerns. Regular audits of data handling practices should be conducted to ensure compliance with privacy standards and regulations.</li>
    
                        <li><strong>Offensive Language or Imagery:</strong> Establishing and enforcing community standards regarding offensive language and imagery is crucial for creating a respectful atmosphere. A clear policy should outline acceptable behavior and the consequences of violating these standards. Continuous monitoring of content, coupled with user feedback mechanisms, will help maintain a healthy community environment.</li>
    
                        <li><strong>Violating Legal Requirements:</strong> The platform should stay updated on relevant laws and regulations, ensuring that all projects comply with legal requirements. This may involve consulting legal experts or regulatory bodies to guide creators and users. Regular training sessions on legal compliance can also be beneficial, helping users understand their obligations and the potential repercussions of non-compliance.</li>
                    </ul>

                    <p>Please take appropriate action to resolve these issues. If you believe this report was made in error, you can contact our support team for further assistance. We value your contributions and want to ensure that your project continues to thrive.</p>
                    
                    <!-- Report Summary Section -->
                    <h2>Report Summary:</h2>
                    <table class=""report-summary"">
                        <tr>
                            <th>Violator Name:</th>
                            <td>{userName}</td>
                        </tr>
                         <tr>
                            <th>Reason:</th>
                            <td>{reasonHtml}</td>
                        </tr>
                        <tr>
                            <th>Report Date:</th>
                            <td>{reportedDate}</td>
                        </tr>
                        <tr>
                            <th>Description:</th>
                            <td>{content}</td>
                        </tr>
                        
                        
                    </table>
                    
                    <!-- Conclusion Section -->
                    <p>We recommend addressing the issue as soon as possible to avoid any impact on your project status. Our team is here to help you navigate this process smoothly.</p>
                    <p>If you have any questions or require further assistance, feel free to reach out to our support team by clicking the button below:</p>
                    <a href=""mailto:fundandfunding2024@gmail.com"" class=""btn"">Contact Support</a>
                    
                    <!-- Footer Section -->
                    <footer>
                        <span>Thank you for your attention to this matter.<br>The Fun&Funding Team</span>
                        <span>&copy; 2024 Fun&Funding. All rights reserved.</span>
                    </footer>
                </div>
            </td>
        </tr>
    </table>
</body>
</html>


";
        }

        private string GenerateMilestoneStatusUpdateEmailBody(string milestoneName, string newStatus, string ownerName)
        {
            return $@"
    <!DOCTYPE html>
    <html lang='en'>
    <head>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <title>Milestone Status Update</title>
        <link href='https://fonts.googleapis.com/css2?family=Poppins:wght@400;600&display=swap' rel='stylesheet'>
        <style>
            body {{ 
                font-family: 'Poppins', sans-serif; 
                background-color: #EAEAEA; 
                color: #2F3645; 
                margin: 0; 
                padding: 20px 0;
            }}
            table {{
                width: 100%;
                height: 100%;
                background-color: #EAEAEA;
                text-align: center;
            }}
            .container {{
                max-width: 450px;
                background-color: #FFFFFF;
                border-radius: 10px;
                padding: 20px;
                border: 1px solid #FFFFFF;
                margin: 30px auto;
                box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
            }}
            h1 {{ 
                color: #2F3645; 
                font-size: 18px;
                margin-bottom: 15px;
            }}
            p {{ 
                line-height: 1.4;
                font-size: 14px;
                color: #2F3645;
                margin-left: 30px;
                margin-right: 30px;
            }}
            .status-badge {{ 
                display: inline-block; 
                padding: 10px 20px;
                font-size: 18px;
                font-weight: 600;
                background-color: #1BAA64; 
                color: #F5F7F8; 
                text-decoration: none; 
                border-radius: 5px; 
                margin-top: 25px;
                width: auto;
            }}
            footer {{ 
                margin-top: 30px;
                font-size: 12px;
                color: #777; 
            }}
        </style>
    </head>
    <body>
        <table>
            <tr>
                <td>
                    <div class='container'>
                        <img src='https://i.ibb.co/SxKvYLH/Frame-155.png' alt='Fun&Funding Logo' width='200px' style='margin-bottom: 20px; margin-top: 10px' />
                        <h1>Your Project Milestone Has Been Updated</h1>
                        <p>Dear {ownerName},</p>
                        <p>We wanted to inform you that the status of your milestone <strong>{milestoneName}</strong> has been updated due to your milestone deadline has been reached. The new status is:</p>
                        <div class='status-badge'>{newStatus}</div>
                        <p>Please log in to your account to review the milestone details and take any necessary actions. If you have any questions or require assistance, feel free to reach out to our support team.</p>
                        <footer>
                            <p>Thank you,<br>The Fun&Funding Team</p>
                            <p>&copy; 2024 Fun&Funding</p>
                        </footer>
                    </div>
                </td>
            </tr>
        </table>
    </body>
    </html>";
        }
        private string GenerateMilestoneReminderEmailBody(string milestoneName, int? daysRemaining, string ownerName)
        {
            return $@"
    <!DOCTYPE html>
    <html lang='en'>
    <head>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <title>Milestone Completion Reminder</title>
        <link href='https://fonts.googleapis.com/css2?family=Poppins:wght@400;600&display=swap' rel='stylesheet'>
        <style>
            body {{ 
                font-family: 'Poppins', sans-serif; 
                background-color: #EAEAEA; 
                color: #2F3645; 
                margin: 0; 
                padding: 20px 0;
            }}
            table {{
                width: 100%;
                height: 100%;
                background-color: #EAEAEA;
                text-align: center;
            }}
            .container {{
                max-width: 450px;
                background-color: #FFFFFF;
                border-radius: 10px;
                padding: 20px;
                border: 1px solid #FFFFFF;
                margin: 30px auto;
                box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
            }}
            h1 {{ 
                color: #2F3645; 
                font-size: 18px;
                margin-bottom: 15px;
            }}
            p {{ 
                line-height: 1.4;
                font-size: 14px;
                color: #2F3645;
                margin-left: 30px;
                margin-right: 30px;
            }}
            .reminder-badge {{ 
                display: inline-block; 
                padding: 10px 20px;
                font-size: 18px;
                font-weight: 600;
                background-color: #FFA500; 
                color: #FFFFFF; 
                text-decoration: none; 
                border-radius: 5px; 
                margin-top: 25px;
                width: auto;
            }}
            footer {{ 
                margin-top: 30px;
                font-size: 12px;
                color: #777; 
            }}
        </style>
    </head>
    <body>
        <table>
            <tr>
                <td>
                    <div class='container'>
                        <img src='https://i.ibb.co/SxKvYLH/Frame-155.png' alt='Fun&Funding Logo' width='200px' style='margin-bottom: 20px; margin-top: 10px' />
                        <h1>Milestone Completion Reminder</h1>
                        <p>Dear {ownerName},</p>
                        <p>This is a reminder that your milestone <strong>{milestoneName}</strong> has <strong>{daysRemaining} days</strong> remaining to complete. Please ensure all necessary actions are taken to meet the milestone requirements by the deadline.</p>
                        <div class='reminder-badge'>{daysRemaining} Days Left</div>
                        <p>Log in to your account to review the milestone progress and make any necessary updates. If you need assistance, please contact our support team.</p>
                        <footer>
                            <p>Thank you,<br>The Fun&Funding Team</p>
                            <p>&copy; 2024 Fun&Funding</p>
                        </footer>
                    </div>
                </td>
            </tr>
        </table>
    </body>
    </html>";
        }
        public async Task SendMilestoneAsync(string toEmail,string projectName, string milestoneName, string ownerName, string? newStatus,int? timeSpan, DateTime reportedDate, EmailType type)
        {
            string body = string.Empty;
            switch (type)
            {
                case EmailType.MilestoneExpired:
                    body = GenerateMilestoneStatusUpdateEmailBody(milestoneName, newStatus, ownerName);
                    break;
                case EmailType.MilestoneReminder:
                    body = GenerateMilestoneReminderEmailBody(milestoneName, timeSpan , ownerName);
                    break;
            }
                
            await _fluentEmail
                .To(toEmail)
                .Subject($@"[UPDATES] - MILESTONE {milestoneName} of PROJECT {projectName} HAS BEEN UPDATED")
                .Body(body, isHtml: true)
                .SendAsync();
        }
    }
}
