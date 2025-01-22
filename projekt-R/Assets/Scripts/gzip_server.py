import http.server
import socketserver
import os

class GzipHTTPRequestHandler(http.server.SimpleHTTPRequestHandler):
    def end_headers(self):
        if self.path.endswith('.gz'):
            self.send_header('Content-Encoding', 'gzip')
        elif self.path.endswith('.br'):
            self.send_header('Content-Encoding', 'br')
        if self.path.endswith('.wasm') or self.path.endswith('.wasm.gz'):
            self.send_header('Content-Type', 'application/wasm')
        super().end_headers()

    def translate_path(self, path):
        base_path = super().translate_path(path)
        if os.path.exists(base_path + '.gz'):
            return base_path + '.gz'
        elif os.path.exists(base_path + '.br'):
            return base_path + '.br'
        return base_path

PORT = 8000
with socketserver.TCPServer(("", PORT), GzipHTTPRequestHandler) as httpd:
    print(f"Serving at port {PORT}")
    httpd.serve_forever()
