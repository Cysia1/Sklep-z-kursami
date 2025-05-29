from flask import Flask, request, jsonify
import os
from uuid import uuid4
import pyodbc

app = Flask(__name__)

conn_str = (
    "DRIVER={ODBC Driver 17 for SQL Server};"
    "SERVER=KIIA_B312_05;"
    "DATABASE=BazaZMultimediami;"
    "Trusted_Connection=yes;"
)

@app.route("/upload-local-blob", methods=["POST"])
def upload_local_blob():
    if 'imageFile' not in request.files or 'courseId' not in request.form or 'courseType' not in request.form:
        return jsonify({"error": "Brak pliku obrazu, CourseId lub CourseType"}), 400

    image_file = request.files['imageFile']
    course_id = request.form['courseId']
    course_type = request.form['courseType']

    if image_file.filename == '':
        return jsonify({"error": "Nie wybrano pliku obrazu"}), 400

    try:
        image_data = image_file.read()
    except Exception as e:
        return jsonify({"error": f"Nie udało się odczytać pliku obrazu: {str(e)}"}), 500

    conn = pyodbc.connect(conn_str)
    cursor = conn.cursor()

    try:
        cursor.execute("""
            UPDATE Courses
            SET Thumbnail = ?, CourseType = ?
            WHERE CourseId = ?
        """, image_data, course_type, course_id)
        conn.commit()
    except pyodbc.Error as ex:
        sqlstate = ex.args[0]
        conn.rollback()
        return jsonify({"error": f"Błąd bazy danych: {sqlstate}"}), 500
    finally:
        cursor.close()
        conn.close()

    return jsonify({"message": "Obrazek zapisany jako BLOB i CourseType zaktualizowane"}), 200

if __name__ == "__main__":
    app.run(debug=True, port=5000)