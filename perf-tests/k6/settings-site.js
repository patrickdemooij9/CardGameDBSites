// SettingsController.GetSiteSettings — calls SettingsService.GetSiteSettings()
// directly with no Lucene search/card mapping in the request, unlike the card-search
// scripts. This isolates the site-settings caching fix from search-cost noise.
import http from 'k6/http';
import { check } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:58529';
const HOST = __ENV.HOST_HEADER || 'localhost:44344';
const VUS = Number(__ENV.VUS || 150);

export const options = {
  scenarios: {
    load: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: '10s', target: VUS },
        { duration: '30s', target: VUS },
        { duration: '10s', target: 0 },
      ],
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.01'],
  },
};

export default function () {
  const res = http.get(`${BASE_URL}/api/settings/site`, {
    headers: { Host: HOST },
  });
  check(res, { 'status is 200': (r) => r.status === 200 });
}
