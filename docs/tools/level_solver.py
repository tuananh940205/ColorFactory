# Bộ giải BFS kiểm tra số nước tối thiểu của LevelData (mô phỏng luật BoardModel).
# Cách dùng: python -I docs/tools/level_solver.py <đường dẫn các file Level_*.asset>
# Bộ giải BFS mô phỏng đúng luật BoardModel: tìm số nước đi tối thiểu để hoàn thành.
import re, sys, random
from collections import deque

COLORS = ["Red", "Blue", "Yellow", "Green", "Purple"]

def decode_hex(h):
    h = h.strip()
    if not h:
        return []
    b = bytes.fromhex(h)
    return [int.from_bytes(b[i:i+4], "little", signed=True) for i in range(0, len(b), 4)]

def parse_level(path):
    text = open(path, encoding="utf-8").read()
    cap = int(re.search(r"trayCapacity: (\d+)", text).group(1))
    trays = []
    for m in re.finditer(r"- colors: ([0-9a-fA-F]*)", text):
        trays.append(tuple(decode_hex(m.group(1))))
    return cap, trays

def encode_hex(colors):
    return "".join(c.to_bytes(4, "little", signed=True).hex() for c in colors)

def is_done(state, cap):
    for t in state:
        if len(t) == 0:
            continue
        if len(t) != cap or any(c != t[0] for c in t):
            return False
    return True

def solve(cap, trays, limit=3_000_000):
    """BFS trên trạng thái đã sắp xếp thứ tự khay (canonical) để giảm không gian tìm kiếm."""
    start = tuple(sorted(tuple(t) for t in trays))
    if is_done(start, cap):
        return 0
    seen = {start}
    q = deque([(start, 0)])
    while q:
        s, d = q.popleft()
        n = len(s)
        for i in range(n):
            if not s[i]:
                continue
            top = s[i][-1]
            for j in range(n):
                if i == j or len(s[j]) >= cap:
                    continue
                if s[j] and s[j][-1] != top:
                    continue
                ns = list(s)
                ns[i] = s[i][:-1]
                ns[j] = s[j] + (top,)
                ns = tuple(sorted(ns))
                if ns in seen:
                    continue
                if is_done(ns, cap):
                    return d + 1
                seen.add(ns)
                if len(seen) > limit:
                    return None
                q.append((ns, d + 1))
    return -1  # không giải được

if __name__ == "__main__":
    # PowerShell không tự mở rộng wildcard cho lệnh native, nên script tự expand glob.
    import glob
    paths = []
    for arg in sys.argv[1:]:
        matched = sorted(glob.glob(arg))
        paths.extend(matched if matched else [arg])
    if not paths:
        sys.exit("Không có file nào để kiểm tra.")
    for lv in paths:
        cap, trays = parse_level(lv)
        print(lv, "cap", cap, "trays", [list(t) for t in trays], "optimal", solve(cap, trays))
