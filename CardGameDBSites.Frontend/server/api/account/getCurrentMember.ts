import { defineEventHandler, getCookie, proxyRequest, sendProxy } from 'h3';

export default defineEventHandler(async (event) => {
  // Read JWT from cookies
  const jwt = getCookie(event, "cardgamesdb"); // 'token' is your cookie name

  // Prepare the backend API URL
  const config = useRuntimeConfig();
  const backendUrl = `${config.public.API_BASE_URL}/api/account/getcurrentmember`;

  // Proxy the request, adding the Authorization header if JWT exists
  const response = await proxyRequest(event, backendUrl, {
    headers: jwt ? { Authorization: `Bearer ${jwt}` } : {},
  });

  return sendProxy(event, response);
});