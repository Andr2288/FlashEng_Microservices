using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashEng.Domain.Exceptions
{
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message) { }
        public BusinessException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class NotFoundException : BusinessException
    {
        public NotFoundException(string entityName, object id)
            : base($"{entityName} with id {id} was not found") { }
    }

    public class BusinessConflictException : BusinessException
    {
        public BusinessConflictException(string message) : base(message) { }
    }

    public class ValidationException : BusinessException
    {
        public ValidationException(string message) : base(message) { }
    }
}
