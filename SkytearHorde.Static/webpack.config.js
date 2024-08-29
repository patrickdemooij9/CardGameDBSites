//webpack.config.js
const path = require('path');

module.exports = {
  mode: "development",
  devtool: "inline-source-map",
  entry: {
    main: "./js/main.ts",
    banner: "./js/banner.ts",
  },
  output: {
    path: path.resolve(__dirname, './../SkytearHorde.Website/wwwroot/js'),
    filename: "[name]-bundle.js"
  },
  resolve: {
    extensions: [".ts", ".tsx", ".js"],
  },
  module: {
    rules: [
      { 
        test: /\.tsx?$/,
        loader: "ts-loader"
      }
    ]
  }
};