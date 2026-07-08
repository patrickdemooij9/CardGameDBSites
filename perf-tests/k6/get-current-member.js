// Authenticated member lookup — AccountApiController.GetCurrentMember.
// Exercises MemberInfoService.GetMemberInfo[Async] (previously a blocking
// _memberManager.GetCurrentMemberAsync().Result call). Simple endpoint,
// so thread-pool starvation effects show up clearly here.
import http from 'k6/http';
import { check } from 'k6';
import { registerAndLogin } from './lib/auth.js';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:58529';
const HOST = __ENV.HOST_HEADER || 'localhost:44344';
const VUS = Number(__ENV.VUS || 100);

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

export function setup() {
  const token = registerAndLogin(BASE_URL, HOST);
  return { token };
}

export default function (data) {
  const res = http.get(`${BASE_URL}/api/account/GetCurrentMember`, {
    headers: { Host: HOST, Authorization: `Bearer ${data.token}` },
  });
  check(res, { 'status is 200': (r) => r.status === 200 });
}
