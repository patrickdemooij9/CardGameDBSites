import { Preferences } from "@capacitor/preferences";

const TOKEN_KEY = "cardgamesdb_jwt";
const ADMIN_TOKEN_KEY = "cardgamesdb_admin_jwt";

let token: string | undefined;
let adminToken: string | undefined;
let loaded = false;

export async function LoadNativeTokens() {
  if (loaded) {
    return;
  }
  token = (await Preferences.get({ key: TOKEN_KEY })).value ?? undefined;
  adminToken = (await Preferences.get({ key: ADMIN_TOKEN_KEY })).value ?? undefined;
  loaded = true;
}

export function GetNativeToken() {
  return token;
}

export function GetNativeAdminToken() {
  return adminToken;
}

export async function SetNativeToken(value: string | undefined) {
  token = value;
  await write(TOKEN_KEY, value);
}

export async function SetNativeAdminToken(value: string | undefined) {
  adminToken = value;
  await write(ADMIN_TOKEN_KEY, value);
}

async function write(key: string, value: string | undefined) {
  if (value) {
    await Preferences.set({ key, value });
  } else {
    await Preferences.remove({ key });
  }
}
