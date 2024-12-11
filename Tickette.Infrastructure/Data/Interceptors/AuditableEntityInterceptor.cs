using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Tickette.Infrastructure.Data.Interceptors;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{

}