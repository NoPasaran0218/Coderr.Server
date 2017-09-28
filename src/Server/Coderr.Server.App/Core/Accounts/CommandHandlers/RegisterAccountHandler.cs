﻿using System;
using System.Threading.Tasks;
using codeRR.Server.Api.Core.Accounts.Commands;
using codeRR.Server.Api.Core.Accounts.Events;
using codeRR.Server.Api.Core.Messaging;
using codeRR.Server.Api.Core.Messaging.Commands;
using codeRR.Server.App.Configuration;
using codeRR.Server.Infrastructure.Configuration;
using DotNetCqs;
using Griffin.Container;

namespace codeRR.Server.App.Core.Accounts.CommandHandlers
{
    /// <summary>
    ///     Register a new account.
    /// </summary>
    [Component]
    public class RegisterAccountHandler : ICommandHandler<RegisterAccount>
    {
        private readonly ICommandBus _commandBus;
        private readonly IEventBus _eventBus;
        private readonly IAccountRepository _repository;

        /// <summary>
        ///     Creates a new instance of <see cref="RegisterAccountHandler" />.
        /// </summary>
        /// <param name="repository">repos</param>
        /// <param name="commandBus">to send verification email</param>
        /// <param name="eventBus">to send <see cref="AccountRegistered" />.</param>
        public RegisterAccountHandler(IAccountRepository repository, ICommandBus commandBus, IEventBus eventBus)
        {
            _repository = repository;
            _commandBus = commandBus;
            _eventBus = eventBus;
        }

        /// <summary>
        ///     Execute a command asynchronously.
        /// </summary>
        /// <param name="command">Command to execute.</param>
        /// <returns>
        ///     Task which will be completed once the command has been executed.
        /// </returns>
        public async Task ExecuteAsync(RegisterAccount command)
        {
            if (command == null) throw new ArgumentNullException("command");

            if (await _repository.IsUserNameTakenAsync(command.UserName))
            {
                await SendAccountInfo(command.UserName);
                return;
            }

            var account = new Account(command.UserName, command.Password);
            account.SetVerifiedEmail(command.Email);
            await _repository.CreateAsync(account);
            await SendVerificationEmail(account);
            var evt = new AccountRegistered(account.Id, account.UserName);
            await _eventBus.PublishAsync(evt);
        }

        private Task SendAccountInfo(string userName)
        {
            //TODO: Send information that states
            //that an account already exist, with instructions on how to reset the account.
            return Task.FromResult<object>(null);
        }

        private Task SendVerificationEmail(Account account)
        {
            var config = ConfigurationStore.Instance.Load<BaseConfiguration>();
            //TODO: HTML email
            var msg = new EmailMessage
            {
                TextBody = string.Format(@"Welcome,


Your activation code is: {0}

You can activate your account by clicking on: {1}/account/activate/{0}

Good luck,
  codeRR Team", account.ActivationKey, config.BaseUrl),
                Subject = "codeRR activation",
                Recipients = new[] {new EmailAddress(account.Email)}
            };

            return _commandBus.ExecuteAsync(new SendEmail(msg));
        }
    }
}