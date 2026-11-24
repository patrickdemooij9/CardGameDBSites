// Must match MinLength on RegisterPostModel and Umbraco:CMS:Security:MemberPassword:RequiredLength.
export const MinPasswordLength = 6;

export function getApiErrorMessage(error: any, fallback: string): string {
  const data = error?.data;
  if (typeof data === "string" && data) return data;

  const validationErrors = data?.errors as Record<string, string[]> | undefined;
  const firstError = validationErrors && Object.values(validationErrors).flat()[0];
  return firstError || fallback;
}
