export const AppLogger = {
  log: (message: string, data?: any) => {
    const isLocalhost = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1';
    if (isLocalhost) {
      console.log(message, data ?? '');
    }
  },

  error: (message: string, errorData?: any) => {
    const isLocalhost = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1';
    if (isLocalhost) {
      console.error(`${message}`, errorData ?? '');
    }
  }
};