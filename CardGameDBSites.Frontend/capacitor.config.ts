import type { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
  appId: 'com.swunlimiteddb.app',
  appName: 'SW Unlimited DB',
  webDir: '.output/public',
  android: {
    allowMixedContent: false
  },
  plugins: {
    SystemBars: {
      insetsHandling: 'native',
      initialViewportFitValueHint: 'cover'
    }
  }
};

export default config;
