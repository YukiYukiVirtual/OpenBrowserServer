#!/usr/bin/env python3
"""VRChat の output_log を模倣して、偽ログファイルへ行を流し込むスクリプト。

OpenBrowserServer.Headless(UBUNTUビルド)の動作確認用。
アプリが監視しているフォルダに output_log_*.txt を新規作成し、
ワールド参加・OpenURL(トークン付き/旧インターフェイス/拒否ケース)の行を
間隔を空けて追記する。

使い方:
  python3 tools/fake_vrchat_log.py --log-dir <アプリの OPENBROWSER_VRCHAT_LOG_DIR>
トークンは http://localhost:21983/ から取得する(--token で直接指定も可)。
"""
import argparse
import json
import os
import sys
import time
import urllib.request


def fetch_token(port):
    with urllib.request.urlopen(f"http://localhost:{port}/", timeout=5) as r:
        return json.load(r)["token"]


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--log-dir", required=True, help="偽ログフォルダ(アプリの監視先)")
    ap.add_argument("--port", type=int, default=21983)
    ap.add_argument("--token", default=None, help="省略時は HTTP でアプリから取得")
    ap.add_argument("--interval", type=float, default=1.0, help="行ごとの間隔(秒)")
    args = ap.parse_args()

    os.makedirs(args.log_dir, exist_ok=True)
    path = os.path.join(args.log_dir, time.strftime("output_log_%Y-%m-%d_%H-%M-%S.txt"))

    # まずファイルを作成する(作成イベントでアプリ側の監視がこのファイルに切り替わる)
    with open(path, "w", encoding="utf-8") as f:
        f.write("2026-11-15 10:00:00 Log        -  [Behaviour] fake log created\n")
    print(f"created: {path}", flush=True)
    time.sleep(1.0)  # アプリ側の追尾スレッド起動待ち

    token = args.token
    if token is None:
        try:
            token = fetch_token(args.port)
        except Exception as e:
            print(f"token取得失敗({e})。ダミートークンで続けます", file=sys.stderr)
            token = "00000000-0000-0000-0000-000000000000"
    print(f"token: {token}", flush=True)

    scenario = [
        ("join-world",
         "2026-11-15 10:00:01 Log        -  [Behaviour] Joining wrld_12345678-1234-1234-1234-123456789abc:12345~public"),
        ("open-allowed",
         f"2026-11-15 10:00:02 Log        -  [YukiYukiVirtual/OpenURL:{token}] https://yukiyukivirtual.net/"),
        ("open-denied-domain",
         f"2026-11-15 10:00:03 Log        -  [YukiYukiVirtual/OpenURL:{token}] https://example.com/"),
        ("open-old-interface",
         "2026-11-15 10:00:04 Log        -  [YukiYukiVirtual/OpenURL] https://booth.pm/"),
        ("open-denied-protocol",
         f"2026-11-15 10:00:05 Log        -  [YukiYukiVirtual/OpenURL:{token}] http://yukiyukivirtual.net/"),
    ]
    with open(path, "a", encoding="utf-8") as f:
        for name, line in scenario:
            f.write(line + "\n")
            f.flush()
            print(f"sent [{name}]: {line}", flush=True)
            time.sleep(args.interval)
    print("done")


if __name__ == "__main__":
    main()
