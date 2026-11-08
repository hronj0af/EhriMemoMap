import postcssPresetEnv from 'postcss-preset-env'

// Fallbacky pro novější CSS (např. inset, logické vlastnosti) ve starších prohlížečích.
export default {
  plugins: [
    postcssPresetEnv({
      browsers: ['defaults', 'safari >= 11', 'ios_saf >= 11', 'chrome >= 61', 'samsung >= 8', 'not IE 11'],
      stage: 2,
    }),
  ],
}
