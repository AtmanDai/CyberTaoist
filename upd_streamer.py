import socket
import json
import time

class HandSignSender:
    """
    一个简单的 UDP 发送器，用于将 CV 识别结果发送给 Unity。
    使用 UDP 是为了最小化延迟，对于实时交互游戏至关重要。
    """
    def __init__(self, ip="127.0.0.1", port=5005):
        self.ip = ip
        self.port = port
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        print(f"[CyberTaoist] UDP Sender initialized on {ip}:{port}")

    def send_prediction(self, sign_id, sign_label):
        """
        发送预测结果。
        
        Args:
            sign_id (int): 手势的索引 ID (例如 0, 1, 2...)
            sign_label (str): 手势的名称 (例如 "Dragon", "Tiger"...)
        """
        # 构造数据包。建议使用 JSON 格式，方便 Unity 解析扩展
        data = {
            "id": sign_id,
            "label": sign_label,
            "timestamp": time.time()
        }
        
        try:
            message = json.dumps(data).encode('utf-8')
            self.sock.sendto(message, (self.ip, self.port))
            # Debug 打印 (可选，生产环境可注释掉)
            # print(f"Sent: {sign_label}") 
        except Exception as e:
            print(f"[Error] Failed to send UDP packet: {e}")

    def close(self):
        self.sock.close()