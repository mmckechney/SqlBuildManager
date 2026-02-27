#!/usr/bin/env python3
"""Render .drawio XML files to PNG using Pillow.

Parses mxGraph XML, extracts cell positions/styles/labels, and draws
boxes with arrows onto a PIL Image.
"""

import os
import re
import xml.etree.ElementTree as ET
from PIL import Image, ImageDraw, ImageFont

INPUT_DIR = os.path.dirname(os.path.abspath(__file__))
SCALE = 2  # render at 2x for crisp output

# Color map: parse fillColor from style strings
DEFAULT_FILL = "#ffffff"
DEFAULT_STROKE = "#333333"
DEFAULT_TEXT = "#333333"

def parse_style(style_str):
    """Parse draw.io style string into a dict."""
    d = {}
    for part in style_str.split(";"):
        if "=" in part:
            k, v = part.split("=", 1)
            d[k.strip()] = v.strip()
    return d

def hex_to_rgb(h):
    h = h.lstrip("#")
    if len(h) == 6:
        return tuple(int(h[i:i+2], 16) for i in (0, 2, 4))
    return (200, 200, 200)

def strip_html(text):
    """Remove HTML tags and decode entities."""
    text = text.replace("&amp;", "&").replace("&lt;", "<").replace("&gt;", ">")
    text = text.replace("&quot;", '"').replace("&#10;", "\n").replace("&apos;", "'")
    text = re.sub(r"<[^>]+>", "", text)
    return text.strip()

def get_font(size, bold=False):
    """Try to load a system font, fall back to default."""
    names = []
    if bold:
        names = ["arialbd.ttf", "Arial Bold.ttf", "DejaVuSans-Bold.ttf", "segoeui.ttf"]
    else:
        names = ["arial.ttf", "Arial.ttf", "DejaVuSans.ttf", "segoeui.ttf"]

    for name in names:
        try:
            return ImageFont.truetype(name, size)
        except (OSError, IOError):
            pass
    # Try Windows font directory
    for name in names:
        try:
            return ImageFont.truetype(os.path.join("C:\\Windows\\Fonts", name), size)
        except (OSError, IOError):
            pass
    return ImageFont.load_default()

def render_drawio(drawio_path, png_path):
    """Parse a .drawio file and render to PNG."""
    tree = ET.parse(drawio_path)
    root = tree.getroot()

    diagram = root.find(".//diagram")
    if diagram is None:
        print(f"  No diagram found in {drawio_path}")
        return False

    model = diagram.find(".//mxGraphModel")
    if model is None:
        print(f"  No mxGraphModel found in {drawio_path}")
        return False

    cells = model.findall(".//mxCell")

    nodes = {}  # id -> {x, y, w, h, label, style_dict}
    edges = []  # [{source, target, label, style_dict}]

    for cell in cells:
        cid = cell.get("id", "")
        value = cell.get("value", "")
        style_str = cell.get("style", "")
        style = parse_style(style_str)

        geom = cell.find("mxGeometry")

        if cell.get("vertex") == "1" and geom is not None:
            x = float(geom.get("x", 0))
            y = float(geom.get("y", 0))
            w = float(geom.get("width", 100))
            h = float(geom.get("height", 40))
            nodes[cid] = {
                "x": x, "y": y, "w": w, "h": h,
                "label": strip_html(value),
                "style": style,
                "style_str": style_str,
            }
        elif cell.get("edge") == "1":
            edges.append({
                "source": cell.get("source", ""),
                "target": cell.get("target", ""),
                "label": strip_html(value),
                "style": style,
            })

    if not nodes:
        print(f"  No nodes found in {drawio_path}")
        return False

    # Compute canvas size
    margin = 30
    max_x = max(n["x"] + n["w"] for n in nodes.values()) + margin
    max_y = max(n["y"] + n["h"] for n in nodes.values()) + margin
    canvas_w = int(max_x + margin) * SCALE
    canvas_h = int(max_y + margin) * SCALE

    img = Image.new("RGB", (canvas_w, canvas_h), (255, 255, 255))
    draw = ImageDraw.Draw(img)

    s = SCALE  # shorthand

    # Draw edges first (behind nodes)
    for edge in edges:
        src = nodes.get(edge["source"])
        tgt = nodes.get(edge["target"])
        if not src or not tgt:
            continue

        # Source bottom center
        sx = int((src["x"] + src["w"] / 2) * s)
        sy = int((src["y"] + src["h"]) * s)
        # Target top center
        tx = int((tgt["x"] + tgt["w"] / 2) * s)
        ty = int(tgt["y"] * s)

        # If target is to the right (side branch), use right edge of source
        src_cx = src["x"] + src["w"] / 2
        tgt_cx = tgt["x"] + tgt["w"] / 2
        src_bot = src["y"] + src["h"]
        tgt_top = tgt["y"]

        # Determine if this is a side connection (target mostly to the right/left)
        dx = abs(tgt_cx - src_cx)
        dy = tgt_top - src_bot

        edge_color = (150, 150, 150)
        line_width = max(1, 1 * s)

        if dx > src["w"] * 0.6 and abs(dy) < src["h"] * 2:
            # Horizontal connection: source right edge -> target left edge
            if tgt_cx > src_cx:
                sx = int((src["x"] + src["w"]) * s)
                sy = int((src["y"] + src["h"] / 2) * s)
                tx = int(tgt["x"] * s)
                ty = int((tgt["y"] + tgt["h"] / 2) * s)
            else:
                sx = int(src["x"] * s)
                sy = int((src["y"] + src["h"] / 2) * s)
                tx = int((tgt["x"] + tgt["w"]) * s)
                ty = int((tgt["y"] + tgt["h"] / 2) * s)

            # Draw orthogonal path: horizontal, vertical, horizontal
            mid_x = (sx + tx) // 2
            draw.line([(sx, sy), (mid_x, sy)], fill=edge_color, width=line_width)
            draw.line([(mid_x, sy), (mid_x, ty)], fill=edge_color, width=line_width)
            draw.line([(mid_x, ty), (tx, ty)], fill=edge_color, width=line_width)
            # Arrowhead
            aw = 4 * s
            if tx > mid_x:
                draw.polygon([(tx, ty), (tx - aw, ty - aw//2), (tx - aw, ty + aw//2)], fill=edge_color)
            else:
                draw.polygon([(tx, ty), (tx + aw, ty - aw//2), (tx + aw, ty + aw//2)], fill=edge_color)
        else:
            # Vertical connection: source bottom -> target top
            sx = int((src["x"] + src["w"] / 2) * s)
            sy = int((src["y"] + src["h"]) * s)
            tx = int((tgt["x"] + tgt["w"] / 2) * s)
            ty = int(tgt["y"] * s)

            # Orthogonal path: down, across, down
            mid_y = (sy + ty) // 2
            draw.line([(sx, sy), (sx, mid_y)], fill=edge_color, width=line_width)
            draw.line([(sx, mid_y), (tx, mid_y)], fill=edge_color, width=line_width)
            draw.line([(tx, mid_y), (tx, ty)], fill=edge_color, width=line_width)
            # Arrowhead pointing down
            aw = 4 * s
            draw.polygon([(tx, ty), (tx - aw//2, ty - aw), (tx + aw//2, ty - aw)], fill=edge_color)

        # Edge label
        if edge["label"]:
            lx = (sx + tx) // 2
            ly = (sy + ty) // 2 - 6 * s
            font_sm = get_font(9 * s)
            bbox = draw.textbbox((0, 0), edge["label"], font=font_sm)
            tw = bbox[2] - bbox[0]
            th = bbox[3] - bbox[1]
            # White background for readability
            draw.rectangle([lx - tw//2 - 2*s, ly - 1*s, lx + tw//2 + 2*s, ly + th + 1*s], fill=(255, 255, 255))
            draw.text((lx - tw//2, ly), edge["label"], fill=(100, 100, 100), font=font_sm)

    # Draw nodes
    for cid, node in nodes.items():
        x = int(node["x"] * s)
        y = int(node["y"] * s)
        w = int(node["w"] * s)
        h = int(node["h"] * s)
        st = node["style"]

        fill_hex = st.get("fillColor", DEFAULT_FILL)
        stroke_hex = st.get("strokeColor", DEFAULT_STROKE)
        fill_rgb = hex_to_rgb(fill_hex)
        stroke_rgb = hex_to_rgb(stroke_hex)

        is_text = "text;" in node["style_str"] or st.get("strokeColor") == "none"
        if is_text:
            # Just render text, no box
            if node["label"]:
                font_size = int(st.get("fontSize", "11")) * s
                font = get_font(font_size, bold="fontStyle=2" in node["style_str"])
                lines = node["label"].split("\n")
                total_h = len(lines) * (font_size + 2*s)
                ty = y + (h - total_h) // 2
                for line in lines:
                    bbox = draw.textbbox((0, 0), line, font=font)
                    tw = bbox[2] - bbox[0]
                    draw.text((x + (w - tw) // 2, ty), line, fill=(100, 100, 100), font=font)
                    ty += font_size + 2*s
            continue

        r = 8 * s  # corner radius
        # Draw rounded rectangle
        draw.rounded_rectangle([x, y, x + w, y + h], radius=r, fill=fill_rgb, outline=stroke_rgb, width=max(1, s))

        # Draw label text
        if node["label"]:
            font_size = int(st.get("fontSize", "11")) * s
            is_bold = "fontStyle=1" in node["style_str"] or st.get("fontStyle") == "1"
            font = get_font(font_size, bold=is_bold)

            lines = node["label"].split("\n")
            line_height = font_size + 3 * s
            total_h = len(lines) * line_height
            ty = y + (h - total_h) // 2

            for line in lines:
                bbox = draw.textbbox((0, 0), line, font=font)
                tw = bbox[2] - bbox[0]
                tx = x + (w - tw) // 2
                draw.text((tx, ty), line, fill=hex_to_rgb(DEFAULT_TEXT), font=font)
                ty += line_height

    img.save(png_path, "PNG")
    return True


def main():
    files = sorted(f for f in os.listdir(INPUT_DIR) if f.endswith(".drawio"))
    print(f"Found {len(files)} .drawio files to render\n")

    for fn in files:
        drawio_path = os.path.join(INPUT_DIR, fn)
        png_fn = fn.replace(".drawio", ".png")
        png_path = os.path.join(INPUT_DIR, png_fn)
        print(f"Rendering {fn} -> {png_fn} ...", end=" ")
        ok = render_drawio(drawio_path, png_path)
        print("OK" if ok else "FAILED")

    print("\nDone!")


if __name__ == "__main__":
    main()
