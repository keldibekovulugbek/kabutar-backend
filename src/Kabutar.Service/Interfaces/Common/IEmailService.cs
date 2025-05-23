﻿using Kabutar.Service.DTOs.Common;

namespace Kabutar.Service.Interfaces.Common;

public interface IEmailService
{
    public Task SendAsync(EmailMessage message);
}
