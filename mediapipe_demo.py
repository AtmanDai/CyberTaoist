#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""
MediaPipe Hand Detection Demo for CyberTaoist
Replaces YOLO model with MediaPipe for hand gesture detection.

Supported gestures (right hand only):
- Rock (defense): Closed fist with thumb tucked in
- Thumbs Up (fireballs): Thumb extended upward, other fingers closed
- Fist (power strike): Closed fist with thumb wrapped around fingers
"""

import argparse
import socket
import json
import time
import cv2 as cv
import mediapipe as mp

# UDP Configuration
UDP_IP = "127.0.0.1"
UDP_PORT = 5005

# Gesture IDs for Unity communication
GESTURE_ROCK = 1      # Defense
GESTURE_THUMBS_UP = 2 # Fireballs
GESTURE_FIST = 3      # Normal attack
GESTURE_NONE = 0      # No gesture detected


def get_args():
    parser = argparse.ArgumentParser()
    parser.add_argument("--device", type=int, default=0)
    parser.add_argument("--width", help='cap width', type=int, default=960)
    parser.add_argument("--height", help='cap height', type=int, default=540)
    parser.add_argument("--fps", type=int, default=30)
    parser.add_argument(
        "--min_detection_confidence",
        type=float,
        default=0.7,
        help="Minimum detection confidence for hand detection",
    )
    parser.add_argument(
        "--min_tracking_confidence",
        type=float,
        default=0.5,
        help="Minimum tracking confidence for hand tracking",
    )
    args = parser.parse_args()
    return args


def is_finger_extended(hand_landmarks, finger_tip_id, finger_pip_id):
    """Check if a finger is extended by comparing tip and pip y coordinates."""
    tip = hand_landmarks.landmark[finger_tip_id]
    pip = hand_landmarks.landmark[finger_pip_id]
    # For vertical orientation, extended finger has lower y value (higher on screen)
    return tip.y < pip.y


def is_thumb_extended(hand_landmarks, handedness):
    """
    Check if thumb is extended based on handedness.
    After cv.flip(), the image is mirrored, so we need to account for that.
    MediaPipe labels hands based on the original (pre-flip) orientation.
    """
    thumb_tip = hand_landmarks.landmark[mp.solutions.hands.HandLandmark.THUMB_TIP]
    thumb_ip = hand_landmarks.landmark[mp.solutions.hands.HandLandmark.THUMB_IP]
    thumb_mcp = hand_landmarks.landmark[mp.solutions.hands.HandLandmark.THUMB_MCP]
    
    # Calculate the horizontal distance between thumb tip and MCP
    # A thumb is extended if the tip is significantly away from the palm center
    thumb_extension = abs(thumb_tip.x - thumb_mcp.x)
    
    # Also check vertical extension for thumbs up gesture
    thumb_up = thumb_tip.y < thumb_ip.y - 0.05  # Thumb pointing upward
    
    # Thumb is extended if there's significant horizontal extension OR pointing up
    return thumb_extension > 0.08 or thumb_up


def is_thumb_up(hand_landmarks):
    """Check if thumb is specifically pointing upward (for thumbs up gesture)."""
    thumb_tip = hand_landmarks.landmark[mp.solutions.hands.HandLandmark.THUMB_TIP]
    thumb_ip = hand_landmarks.landmark[mp.solutions.hands.HandLandmark.THUMB_IP]
    thumb_mcp = hand_landmarks.landmark[mp.solutions.hands.HandLandmark.THUMB_MCP]
    
    # Thumb is pointing up if tip is significantly higher (lower y) than the MCP
    vertical_extension = thumb_mcp.y - thumb_tip.y
    return vertical_extension > 0.1


def detect_gesture(hand_landmarks, handedness):
    """
    Detect hand gesture from landmarks.
    Returns gesture ID and name.
    
    Gestures (right hand only for spell casting):
    - Thumbs Up: Thumb pointing upward, other fingers closed -> Fireballs
    - Fist: All fingers closed including thumb wrapped around -> Power Strike
    - Rock: Similar to fist but used when holding position -> Defense
    
    To differentiate Fist and Rock:
    - Rock: Detected when hand is relatively stationary (defensive stance)
    - Fist: Detected as the default closed hand gesture
    
    Since we can't easily detect motion, we use thumb position:
    - Thumb tucked in tightly (Rock/Defense)
    - Thumb wrapped around fingers loosely (Fist/Power Strike)
    """
    mp_hands = mp.solutions.hands
    
    # Check each finger
    index_extended = is_finger_extended(
        hand_landmarks, 
        mp_hands.HandLandmark.INDEX_FINGER_TIP,
        mp_hands.HandLandmark.INDEX_FINGER_PIP
    )
    middle_extended = is_finger_extended(
        hand_landmarks,
        mp_hands.HandLandmark.MIDDLE_FINGER_TIP,
        mp_hands.HandLandmark.MIDDLE_FINGER_PIP
    )
    ring_extended = is_finger_extended(
        hand_landmarks,
        mp_hands.HandLandmark.RING_FINGER_TIP,
        mp_hands.HandLandmark.RING_FINGER_PIP
    )
    pinky_extended = is_finger_extended(
        hand_landmarks,
        mp_hands.HandLandmark.PINKY_TIP,
        mp_hands.HandLandmark.PINKY_PIP
    )
    thumb_extended = is_thumb_extended(hand_landmarks, handedness)
    thumb_up = is_thumb_up(hand_landmarks)
    
    # Count extended fingers (excluding thumb)
    fingers_extended = sum([index_extended, middle_extended, ring_extended, pinky_extended])
    
    # Gesture classification for right hand (spell casting hand)
    if handedness == "Right":
        # Thumbs Up: Thumb pointing upward, other fingers closed
        if thumb_up and fingers_extended == 0:
            return GESTURE_THUMBS_UP, "Thumbs Up"
        
        # Check for closed fist variations
        if fingers_extended == 0:
            # Fist with thumb extended outward (Power Strike - offensive)
            if thumb_extended:
                return GESTURE_FIST, "Fist"
            # Fist with thumb tucked in (Rock - defensive)
            else:
                return GESTURE_ROCK, "Rock"
    
    return GESTURE_NONE, "None"


def main():
    args = get_args()
    cap_device = args.device
    cap_width = args.width
    cap_height = args.height
    fps = args.fps
    
    # Initialize UDP socket
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    print(f"[CyberTaoist] UDP Sender initialized on {UDP_IP}:{UDP_PORT}")
    
    # Initialize camera
    cap = cv.VideoCapture(cap_device)
    cap.set(cv.CAP_PROP_FRAME_WIDTH, cap_width)
    cap.set(cv.CAP_PROP_FRAME_HEIGHT, cap_height)
    
    # Initialize MediaPipe Hands
    mp_hands = mp.solutions.hands
    mp_drawing = mp.solutions.drawing_utils
    mp_drawing_styles = mp.solutions.drawing_styles
    
    hands = mp_hands.Hands(
        static_image_mode=False,
        max_num_hands=2,
        min_detection_confidence=args.min_detection_confidence,
        min_tracking_confidence=args.min_tracking_confidence
    )
    
    print("[CyberTaoist] MediaPipe Hand Detection Started")
    print("Gestures: Rock (Defense), Thumbs Up (Fireballs), Fist (Power Strike)")
    print("Use left hand index finger for player movement")
    print("Use right hand for spell casting")
    print("Press ESC to exit")
    
    while True:
        start_time = time.time()
        
        ret, frame = cap.read()
        if not ret:
            continue
        
        # Flip frame horizontally for a mirror view
        frame = cv.flip(frame, 1)
        debug_image = frame.copy()
        
        # Convert BGR to RGB for MediaPipe
        rgb_frame = cv.cvtColor(frame, cv.COLOR_BGR2RGB)
        rgb_frame.flags.writeable = False
        
        # Process frame with MediaPipe
        results = hands.process(rgb_frame)
        
        rgb_frame.flags.writeable = True
        
        # Data to send to Unity
        left_index_x = -1.0
        left_index_y = -1.0
        right_gesture_id = GESTURE_NONE
        right_gesture_name = "None"
        
        if results.multi_hand_landmarks:
            for hand_landmarks, handedness_info in zip(
                results.multi_hand_landmarks, 
                results.multi_handedness
            ):
                # Get handedness (Left or Right)
                handedness = handedness_info.classification[0].label
                
                # Draw hand landmarks
                mp_drawing.draw_landmarks(
                    debug_image,
                    hand_landmarks,
                    mp_hands.HAND_CONNECTIONS,
                    mp_drawing_styles.get_default_hand_landmarks_style(),
                    mp_drawing_styles.get_default_hand_connections_style()
                )
                
                if handedness == "Left":
                    # Get left index finger tip position for player movement
                    index_tip = hand_landmarks.landmark[mp_hands.HandLandmark.INDEX_FINGER_TIP]
                    left_index_x = index_tip.x  # Normalized 0-1
                    left_index_y = index_tip.y  # Normalized 0-1
                    
                    # Draw index finger position
                    h, w = debug_image.shape[:2]
                    cx, cy = int(index_tip.x * w), int(index_tip.y * h)
                    cv.circle(debug_image, (cx, cy), 15, (0, 255, 0), -1)
                    cv.putText(
                        debug_image,
                        f"Move: ({left_index_x:.2f}, {left_index_y:.2f})",
                        (cx + 20, cy),
                        cv.FONT_HERSHEY_SIMPLEX,
                        0.6,
                        (0, 255, 0),
                        2
                    )
                
                elif handedness == "Right":
                    # Detect gesture from right hand
                    gesture_id, gesture_name = detect_gesture(hand_landmarks, handedness)
                    right_gesture_id = gesture_id
                    right_gesture_name = gesture_name
                    
                    # Draw gesture info
                    wrist = hand_landmarks.landmark[mp_hands.HandLandmark.WRIST]
                    h, w = debug_image.shape[:2]
                    wx, wy = int(wrist.x * w), int(wrist.y * h)
                    
                    # Color based on gesture
                    if gesture_id == GESTURE_ROCK:
                        color = (255, 215, 0)  # Gold for defense
                    elif gesture_id == GESTURE_THUMBS_UP:
                        color = (0, 0, 255)  # Red for fireballs
                    elif gesture_id == GESTURE_FIST:
                        color = (255, 255, 255)  # White for normal attack
                    else:
                        color = (128, 128, 128)  # Gray for none
                    
                    cv.putText(
                        debug_image,
                        f"Spell: {gesture_name}",
                        (wx - 50, wy - 30),
                        cv.FONT_HERSHEY_SIMPLEX,
                        0.8,
                        color,
                        2
                    )
        
        # Send data to Unity via UDP
        data = {
            "left_index_x": left_index_x,
            "left_index_y": left_index_y,
            "gesture_id": right_gesture_id,
            "gesture_name": right_gesture_name,
            "timestamp": time.time()
        }
        
        try:
            message = json.dumps(data).encode('utf-8')
            sock.sendto(message, (UDP_IP, UDP_PORT))
        except Exception as e:
            print(f"[Error] Failed to send UDP packet: {e}")
        
        # Calculate and display FPS
        elapsed_time = time.time() - start_time
        actual_fps = 1.0 / elapsed_time if elapsed_time > 0 else 0
        
        cv.putText(
            debug_image,
            f"FPS: {actual_fps:.1f} | Elapsed: {elapsed_time*1000:.1f}ms",
            (10, 30),
            cv.FONT_HERSHEY_SIMPLEX,
            0.8,
            (0, 255, 0),
            2
        )
        
        # Display hand info
        info_y = 60
        cv.putText(
            debug_image,
            f"Left Index: ({left_index_x:.2f}, {left_index_y:.2f})",
            (10, info_y),
            cv.FONT_HERSHEY_SIMPLEX,
            0.6,
            (255, 255, 0),
            2
        )
        info_y += 25
        cv.putText(
            debug_image,
            f"Right Gesture: {right_gesture_name} (ID: {right_gesture_id})",
            (10, info_y),
            cv.FONT_HERSHEY_SIMPLEX,
            0.6,
            (255, 255, 0),
            2
        )
        
        # Show frame
        cv.imshow('CyberTaoist MediaPipe Demo', debug_image)
        
        # Control FPS
        sleep_time = max(0, (1.0 / fps) - elapsed_time)
        time.sleep(sleep_time)
        
        # Exit on ESC
        key = cv.waitKey(1)
        if key == 27:
            break
    
    hands.close()
    cap.release()
    cv.destroyAllWindows()
    sock.close()
    print("[CyberTaoist] MediaPipe Hand Detection Stopped")


if __name__ == '__main__':
    main()
