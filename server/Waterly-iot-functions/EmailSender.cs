using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Net;
using System.Net.Mail;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html.simpleparser;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Waterly_iot_functions
{
    class EmailSender
    {

        private static Dictionary<int, string> monthDict = new Dictionary<int, string>()
        {
            [1] = "January",
            [2] = "February",
            [3] = "March",
            [4] = "April",
            [5] = "May",
            [6] = "June",
            [7] = "July",
            [8] = "August",
            [9] = "September",
            [10] = "October",
            [11] = "November",
            [12] = "December"
        };

        public static string siteUrl = "https://waterly.azurewebsites.net/";


        public static void sendMail(string subject, string mailBody, string reciever)
        {
            //todo: delete this after testing
            if (!reciever.Equals("iot.waterly@gmail.com"))
            {
                return;
            }

            var doc = new Document();
            MemoryStream memoryStream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(doc, memoryStream);

            doc.Open();
            doc.Add(new Paragraph(mailBody));

            writer.CloseStream = false;
            doc.Close();
            memoryStream.Position = 0;

            MailMessage mm = new MailMessage("iot.waterly@gmail.com", reciever)
            {
                Subject = subject,
                IsBodyHtml = false,
                Body = mailBody
            };

            mm.Attachments.Add(new Attachment(memoryStream, "attachment.pdf"));
            SmtpClient smtp = new SmtpClient
            {
                UseDefaultCredentials = false,
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential("iot.waterly@gmail.com", "ytqhegrwwjjbhpft")
            };
            smtp.Send(mm);
            Console.WriteLine($"Mail sent succesfuly to {reciever}");
        }

        public static void sendMailMontlyBill(BillItem billItem, UserItem userItem)
        {
            string subject = $"Your monthly water bill has arrived - {getMonth(billItem.month)} {billItem.year}";
            string mailBody = $"Hey {userItem.full_name}, \n" +
                $"Your monthly bill for {getMonth(billItem.month)} {billItem.year} has just arrived! \n\n" +
                $"Total consumption this month: {billItem.total_flow/1000000} cubic meters \n" +
                $"Water Expenses: {billItem.water_expenses}$ \n" +
                $"Fixed Expenses: {billItem.fixed_expenses}$ \n" +
                $"Total: {billItem.water_expenses + billItem.fixed_expenses}$ \n\n" +
                $"Please go over to {siteUrl} to pay it. \n\n" +
                $"Thanks for using WATERLY!\n" +
                $"WATERLY team.";
            string reciever = userItem.email;
            Console.WriteLine($"Sending mail - monthly bill to {reciever}");
            sendMail(subject, mailBody, reciever);
        }

        public static void sendMailBillPaymentConfirmation(BillItem billItem, string userId)
        {
            string subject = $"Confirmation Mail for Bill Payment - {getMonth(billItem.month)} {billItem.year}";
            string mailBody = $"Thanks!\n\n" +
                $"You just paid your {getMonth(billItem.month)} {billItem.year} water bill.\n" +
                $"Your payment was proccessed successfully.\n\n" +
                $"Thanks!\n" +
                $"WATERLY team.";
            string reciever = getEmailAddresByUserId(userId).Result;
            Console.WriteLine($"Sending mail - payment confirmation to {reciever}");
            sendMail(subject, mailBody, reciever);
        }

        public static void sendMailNewAlert(AlertItem alertItem, string userId)
        {
            string subject = $"Attention! We have just detected an abnormal values in your water system.";
            string mailBody = $"Dear WATERLY user,\n\n" +
                $"We have just detected an abnormal values in your water system.\n" +
                $"Issue detected: {alertItem.type}.\n" +
                $"{alertItem.message}.\n" +
                $"After finding out, we will be thankful if you could give us feedback " +
                $"on this alert in the alerts section in your account.\n\n" +
                $"Thanks!\n" +
                $"WATERLY team.";
            string reciever = getEmailAddresByUserId(userId).Result;
            Console.WriteLine($"Sending mail - new alert to {reciever}");
            sendMail(subject, mailBody, reciever);
        }


        public static string getMonth(int month)
        {
            return monthDict[month];
        }

        public static async Task<string> getEmailAddresByUserId(string userId)
        {
            Console.WriteLine("Getting email address...");

            var sqlQueryText = $"SELECT TOP 1 * FROM c WHERE c.id = '{userId}'";
            string email = "";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<UserItem> queryResultSetIterator = Resources.users_container.GetItemQueryIterator<UserItem>(queryDefinition);
            FeedResponse<UserItem> currentResultSet;
            while (queryResultSetIterator.HasMoreResults)
            {
                currentResultSet = await queryResultSetIterator.ReadNextAsync();
                UserItem user = currentResultSet.FirstOrDefault<UserItem>();
                email = user.email;
            }
            return email;
        }

    }
}
