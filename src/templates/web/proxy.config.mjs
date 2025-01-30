// See: https://angular.dev/tools/cli/serve#proxying-to-a-backend-server

export default [
  {
    context: "/api",
    target: `http://applicationname.kube.local`,
    secure: false,
    changeOrigin: true,
    logLevel: "debug",
  },
  {
    context: "/api-hub",
    target: `http://applicationname.kube.local`,
    ws: true,
    secure: false,
    changeOrigin: true,
    logLevel: "debug",
  },
];
