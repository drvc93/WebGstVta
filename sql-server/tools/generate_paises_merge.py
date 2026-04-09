# -*- coding: utf-8 -*-
"""Genera MERGE SQL desde sql-server/data/paises.csv. Ejecutar: python generate_paises_merge.py > ../06_SeedPaisesMerge.sql"""
import csv
import os
import sys

def esc(s: str) -> str:
    if s is None:
        return "NULL"
    s = s.strip()
    if not s:
        return "NULL"
    return "N'" + s.replace("'", "''") + "'"

def main():
    root = os.path.dirname(os.path.abspath(__file__))
    path = os.path.join(root, "..", "data", "paises.csv")
    out_path = os.path.join(root, "..", "06_SeedPaisesMerge.sql")
    rows = []
    with open(path, encoding="utf-8-sig", newline="") as f:
        reader = csv.DictReader(f)
        for raw in reader:
            r = {((k or "").strip()): (v or "").strip() if isinstance(v, str) else v for k, v in raw.items()}
            iso2 = (r.get("iso2") or "").strip()
            iso3 = (r.get("iso3") or "").strip()
            if not iso2:
                continue
            if iso3 == iso2 and len(iso2) == 2:
                iso3 = ""
            nombre = (r.get("nombre") or "").strip()
            name_en = (r.get("name") or "").strip()
            phone = (r.get("phone_code") or "").strip()
            cont = (r.get("continente") or "").strip()
            rows.append((iso2, nombre, name_en, iso3 or None, phone or None, cont or None))

    lines = []
    lines.append("-- Auto-generado por tools/generate_paises_merge.py (no editar a mano)")
    lines.append("USE GestVta;")
    lines.append("GO")
    lines.append("SET NOCOUNT ON;")
    lines.append("")
    lines.append("IF COL_LENGTH(N'dbo.Pais', N'NombreEn') IS NULL")
    lines.append("BEGIN")
    lines.append("    ALTER TABLE dbo.Pais ALTER COLUMN Nombre NVARCHAR(200) NOT NULL;")
    lines.append("    ALTER TABLE dbo.Pais ADD")
    lines.append("        NombreEn NVARCHAR(200) NULL,")
    lines.append("        Iso3 NVARCHAR(3) NULL,")
    lines.append("        TelefonoCodigo NVARCHAR(40) NULL,")
    lines.append("        Continente NVARCHAR(80) NULL;")
    lines.append("END")
    lines.append("GO")
    lines.append("")
    lines.append("DECLARE @src TABLE (")
    lines.append("    Codigo NVARCHAR(5) NOT NULL,")
    lines.append("    Nombre NVARCHAR(200) NOT NULL,")
    lines.append("    NombreEn NVARCHAR(200) NULL,")
    lines.append("    Iso3 NVARCHAR(3) NULL,")
    lines.append("    TelefonoCodigo NVARCHAR(40) NULL,")
    lines.append("    Continente NVARCHAR(80) NULL,")
    lines.append("    Activo BIT NOT NULL")
    lines.append(");")
    lines.append("")

    for iso2, nombre, name_en, iso3, phone, cont in rows:
        i3 = esc(iso3) if iso3 else "NULL"
        ph = esc(phone) if phone else "NULL"
        co = esc(cont) if cont else "NULL"
        ne = esc(name_en) if name_en else "NULL"
        lines.append(
            f"INSERT INTO @src (Codigo, Nombre, NombreEn, Iso3, TelefonoCodigo, Continente, Activo) VALUES "
            f"({esc(iso2)}, {esc(nombre)}, {ne}, {i3}, {ph}, {co}, 1);"
        )

    lines.append("")
    lines.append("MERGE dbo.Pais AS t")
    lines.append("USING @src AS s ON t.Codigo = s.Codigo")
    lines.append("WHEN MATCHED THEN UPDATE SET")
    lines.append("    t.Nombre = s.Nombre,")
    lines.append("    t.NombreEn = s.NombreEn,")
    lines.append("    t.Iso3 = s.Iso3,")
    lines.append("    t.TelefonoCodigo = s.TelefonoCodigo,")
    lines.append("    t.Continente = s.Continente,")
    lines.append("    t.Activo = s.Activo")
    lines.append("WHEN NOT MATCHED THEN")
    lines.append("    INSERT (Codigo, Nombre, NombreEn, Iso3, TelefonoCodigo, Continente, Activo)")
    lines.append("    VALUES (s.Codigo, s.Nombre, s.NombreEn, s.Iso3, s.TelefonoCodigo, s.Continente, s.Activo);")
    lines.append("GO")
    lines.append("PRINT N'Paises MERGE completado.';")
    lines.append("GO")

    with open(out_path, "w", encoding="utf-8", newline="\n") as out:
        out.write("\n".join(lines) + "\n")
    print(f"Written {len(rows)} rows to {out_path}", file=sys.stderr)

if __name__ == "__main__":
    main()
