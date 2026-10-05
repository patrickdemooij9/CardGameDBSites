export default defineEventHandler(async (event) => {
  const secret = useRuntimeConfig(event).cachePurgeSecret;
  if (!secret || getHeader(event, 'x-purge-secret') !== secret) {
    throw createError({ statusCode: 401 });
  }

  const storage = useStorage('cache');
  const keys = await storage.getKeys('nitro:routes');
  await Promise.all(keys.map((key) => storage.removeItem(key)));

  return { purged: keys.length };
});
