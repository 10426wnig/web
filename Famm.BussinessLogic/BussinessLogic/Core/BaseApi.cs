using System;

namespace Famm.BussinessLogic.BussinessLogic.Core
{
    public abstract class BaseApi : IDisposable
    {
        private bool _disposed = false;
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Освобождаем управляемые ресурсы
                }
                
                // Освобождаем неуправляемые ресурсы
                _disposed = true;
            }
        }
    }
} 