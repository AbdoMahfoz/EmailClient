﻿using IMAPLayer.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IMAPLayer
{
    public interface IMailServer : IDisposable
    {
        public int MailCount { get; }
        Task<bool> Login(string email, string password);
        Task<IEnumerable<MailNode>> GetMailTree();
        Task<bool> SelectMailBox(string MailBox);
        Task<IEnumerable<MailHeaderObject>> GetMails(int count);
        Task<Dictionary<string, string>> GetMail(int Id);
    }
}