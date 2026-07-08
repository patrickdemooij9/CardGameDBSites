// Anonymous card search — CardApiController.Query with no auth/collection filter.
// This is the highest-traffic endpoint on the site and a good general-purpose
// regression check, but it does NOT exercise the member-lookup branch
// (see card-search-collection-sort.js for that).
import http from 'k6/http';
import { check } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:58529';
const HOST = __ENV.HOST_HEADER || 'localhost:44344';
const VUS = Number(__ENV.VUS || 50);
const PAGE_SIZE = Number(__ENV.PAGE_SIZE || 20);

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
  const res = http.post(
    `${BASE_URL}/api/cards/query`,
    JSON.stringify({
      pageSize: PAGE_SIZE,
      pageNumber: Math.floor(Math.random() * 10) + 1,
      filterClauses: [],
    }),
    { headers: { 'Content-Type': 'application/json', Host: HOST } }
  );
  check(res, { 'status is 200': (r) => r.status === 200 });
}
