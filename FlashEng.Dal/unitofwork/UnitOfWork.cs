using FlashEng.Dal.Interfaces;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FlashEng.Dal.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly string _usersConnectionString;
        private readonly string _flashcardsConnectionString;
        private readonly string _ordersConnectionString;

        private MySqlConnection? _usersConnection;
        private MySqlConnection? _flashcardsConnection;
        private MySqlConnection? _ordersConnection;

        private MySqlTransaction? _usersTransaction;
        private MySqlTransaction? _flashcardsTransaction;
        private MySqlTransaction? _ordersTransaction;

        private IUserRepository? _users;
        private IFlashcardRepository? _flashcards;
        private IOrderRepository? _orders;

        public UnitOfWork(
            string usersConnectionString,
            string flashcardsConnectionString,
            string ordersConnectionString)
        {
            _usersConnectionString = usersConnectionString;
            _flashcardsConnectionString = flashcardsConnectionString;
            _ordersConnectionString = ordersConnectionString;
        }

        public IUserRepository Users
        {
            get
            {
                return _users ??= new Repositories.UserRepository(_usersConnectionString);
            }
        }

        public IFlashcardRepository Flashcards
        {
            get
            {
                return _flashcards ??= new Repositories.FlashcardRepository(_flashcardsConnectionString);
            }
        }

        public IOrderRepository Orders
        {
            get
            {
                return _orders ??= new Repositories.OrderRepository(_ordersConnectionString);
            }
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            // Відкриваємо з'єднання та починаємо транзакції для всіх БД
            _usersConnection = new MySqlConnection(_usersConnectionString);
            await _usersConnection.OpenAsync(cancellationToken);
            _usersTransaction = await _usersConnection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

            _flashcardsConnection = new MySqlConnection(_flashcardsConnectionString);
            await _flashcardsConnection.OpenAsync(cancellationToken);
            _flashcardsTransaction = await _flashcardsConnection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

            _ordersConnection = new MySqlConnection(_ordersConnectionString);
            await _ordersConnection.OpenAsync(cancellationToken);
            _ordersTransaction = await _ordersConnection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_usersTransaction != null)
                    await _usersTransaction.CommitAsync(cancellationToken);

                if (_flashcardsTransaction != null)
                    await _flashcardsTransaction.CommitAsync(cancellationToken);

                if (_ordersTransaction != null)
                    await _ordersTransaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_usersTransaction != null)
                    await _usersTransaction.RollbackAsync(cancellationToken);
            }
            catch { }

            try
            {
                if (_flashcardsTransaction != null)
                    await _flashcardsTransaction.RollbackAsync(cancellationToken);
            }
            catch { }

            try
            {
                if (_ordersTransaction != null)
                    await _ordersTransaction.RollbackAsync(cancellationToken);
            }
            catch { }
        }

        public void Dispose()
        {
            _usersTransaction?.Dispose();
            _flashcardsTransaction?.Dispose();
            _ordersTransaction?.Dispose();

            _usersConnection?.Dispose();
            _flashcardsConnection?.Dispose();
            _ordersConnection?.Dispose();
        }
    }
}
