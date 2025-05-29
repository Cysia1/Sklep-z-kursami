from flask import Flask, request, jsonify
import base64
import pyodbc

app = Flask(__name__)

conn_str = (
    "DRIVER={ODBC Driver 17 for SQL Server};"
    "SERVER=BANG-OLUFSEN;"
    "DATABASE=BazaZMultimediami;"
    "Trusted_Connection=yes;"
)

@app.route("/upload-local-blob", methods=["POST"])
def upload_local_blob():
    if ('imageFile' not in request.files or
        'courseId' not in request.form or
        'courseType' not in request.form):
        return jsonify({"error": "Brak pliku obrazu, CourseId lub CourseType"}), 400

    image_file = request.files['imageFile']
    course_id = request.form['courseId']
    course_type = request.form['courseType']

    if image_file.filename == '':
        return jsonify({"error": "Nie wybrano pliku obrazu"}), 400

    # Odczytaj dane obrazu
    try:
        image_data = image_file.read()
    except Exception as e:
        return jsonify({"error": f"Nie udało się odczytać pliku obrazu: {str(e)}"}), 500

    # Odczytaj plik PDF, jeśli jest przesłany
    pdf_data = None
    if 'pdfFile' in request.files:
        pdf_file = request.files['pdfFile']
        if pdf_file.filename != '':
            try:
                pdf_data = pdf_file.read()
            except Exception as e:
                return jsonify({"error": f"Nie udało się odczytać pliku PDF: {str(e)}"}), 500

    try:
        conn = pyodbc.connect(conn_str)
        cursor = conn.cursor()

        if pdf_data is not None:
            # Jeśli jest PDF, aktualizuj też pole PdfFile
            cursor.execute("""
                UPDATE Courses
                SET Thumbnail = ?, CourseType = ?, PdfFile = ?
                WHERE CourseId = ?
            """, image_data, course_type, pdf_data, course_id)
        else:
            # Jeśli PDF nie ma, aktualizuj tylko obraz i typ kursu
            cursor.execute("""
                UPDATE Courses
                SET Thumbnail = ?, CourseType = ?
                WHERE CourseId = ?
            """, image_data, course_type, course_id)

        conn.commit()
    except pyodbc.Error as ex:
        conn.rollback()
        return jsonify({"error": f"Błąd bazy danych: {ex}"}), 500
    finally:
        cursor.close()
        conn.close()

    thumbnail_base64 = base64.b64encode(image_data).decode('utf-8')
    return jsonify({
        "message": "Obrazek (i ewentualnie PDF) zapisane jako BLOB, CourseType zaktualizowane",
        "id": thumbnail_base64
    }), 200

if __name__ == "__main__":
    app.run(debug=True, port=5000)
