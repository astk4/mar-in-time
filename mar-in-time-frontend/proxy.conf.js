const PROXY_CONFIG = [
  {
    context: [
        '/hubs/ais',
    ],
    target: 'http://host.docker.internal:7120',
    secure: false,
    ws: true, //websocket
    logLevel: "debug"
}
]

module.exports = PROXY_CONFIG;