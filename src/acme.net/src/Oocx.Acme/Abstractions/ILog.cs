﻿namespace Oocx.Acme.Services
{
    public interface ILog
    {
        void Verbose(string message);

        void Info(string message);

        void Warning(string message);

        void Error(string message);
    }
}
