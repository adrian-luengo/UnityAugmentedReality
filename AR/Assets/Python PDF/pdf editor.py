import fitz
from PIL import Image
import io

# Rutas de entrada/salida reestablecidas
input_pdf_path = "C:/Users/User/Downloads/Fulton Catalog-Spanish.pdf"
logo_path = "C:/Users/User/Downloads/WhatsApp Image 2025-05-22 at 19.45.18.jpeg"
output_pdf_path = "C:/Users/User/Downloads/Catalogo_Sin_Chino_Con_Nuevo_Logo.pdf"

# Cargar el logo
logo_img = Image.open(logo_path)
logo_bytes = io.BytesIO()
logo_img.save(logo_bytes, format='PNG')
logo_bytes.seek(0)

# Abrir PDF
doc = fitz.open(input_pdf_path)

# Palabras clave para eliminar (marca de agua + caracteres chinos)
keywords_to_erase = [
    "Jining Fulton Machinery Manufacturing Co.",
    "济宁富陆通机械设备有限公司",
    "富陆通"
]

# Procesar todas las páginas
for page in doc:
    blocks = page.get_text("dict")["blocks"]
    for block in blocks:
        for line in block.get("lines", []):
            for span in line.get("spans", []):
                # Comprobar si contiene texto chino o palabras clave
                if any(kw in span["text"] for kw in keywords_to_erase) or any('\u4e00' <= ch <= '\u9fff' for ch in span["text"]):
                    # Ocultar el texto con un rectángulo blanco
                    rect = fitz.Rect(span["bbox"])
                    page.draw_rect(rect, color=(1, 1, 1), fill=(1, 1, 1))

    # Insertar el nuevo logo en la parte inferior derecha
    page.insert_image(
        fitz.Rect(page.rect.width - 120, 20, page.rect.width - 20, 100),
        stream=logo_bytes.getvalue(),
        keep_proportion=True
    )

# Guardar el documento editado
doc.save(output_pdf_path)
doc.close()

output_pdf_path