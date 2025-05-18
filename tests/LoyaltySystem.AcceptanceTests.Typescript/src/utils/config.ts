export type Environment = 'local' | 'docker';

export const config = {
  local: {
    mailhogUrl: 'http://localhost:8025/',
    adminApiUrl: 'http://localhost:5001/',
    staffApiUrl: 'http://localhost:5003/',
  },
  docker: {
    mailhogUrl: 'http://mailhog:8025/',
    adminApiUrl: 'http://admin-api:5001/',
    staffApiUrl: 'http://staff-api:5003/',
  },
};

export const currentEnv: Environment = (process.env.TEST_ENV as Environment) || 'local';

export const envConfig = config[currentEnv]; 