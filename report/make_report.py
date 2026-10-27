# -*- coding: utf-8 -*-
"""Генерация отчёта по лабораторной работе №2 в формате .docx
по оформлению отчёта лабораторной работы №1 (ЧелГУ, Times New Roman, GOST-style)."""
import csv, os
import docx
from docx import Document
from docx.shared import Pt, Cm, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_LINE_SPACING
from docx.enum.table import WD_TABLE_ALIGNMENT

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
RESULTS = os.path.join(ROOT, "results")

doc = Document()

# --- базовые настройки страницы и стилей (как в отчёте лабы №1) ---
sec = doc.sections[0]
sec.left_margin, sec.right_margin = Cm(3), Cm(1)
sec.top_margin, sec.bottom_margin = Cm(2), Cm(2)

style = doc.styles["Normal"]
style.font.name = "Times New Roman"
style.font.size = Pt(14)
pf = style.paragraph_format
pf.line_spacing_rule = WD_LINE_SPACING.ONE_POINT_FIVE
pf.space_after = Pt(0)

for name, size in [("Heading 1", 16), ("Heading 2", 15), ("Heading 3", 14)]:
    h = doc.styles[name]
    h.font.name = "Times New Roman"
    h.font.size = Pt(size)
    h.font.bold = True
    h.font.color.rgb = RGBColor(0, 0, 0)

def body(text, bold=False, align=WD_ALIGN_PARAGRAPH.JUSTIFY, indent=True, size=14):
    p = doc.add_paragraph()
    p.alignment = align
    if indent:
        p.paragraph_format.first_line_indent = Cm(1.25)
    r = p.add_run(text)
    r.font.name = "Times New Roman"
    r.font.size = Pt(size)
    r.bold = bold
    return p

def bullet(text):
    p = body(text, align=WD_ALIGN_PARAGRAPH.JUSTIFY)
    p.paragraph_format.first_line_indent = Cm(1.25)
    return p

def code(lines):
    for ln in lines:
        p = doc.add_paragraph()
        p.paragraph_format.line_spacing_rule = WD_LINE_SPACING.SINGLE
        p.paragraph_format.space_after = Pt(0)
        p.paragraph_format.left_indent = Cm(1)
        r = p.add_run(ln if ln else " ")
        r.font.name = "Consolas"
        r.font.size = Pt(11)

def caption(text):
    p = body(text, align=WD_ALIGN_PARAGRAPH.CENTER, indent=False, size=12)
    return p

def figure(png, cap):
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p.add_run().add_picture(os.path.join(RESULTS, png), width=Cm(15.5))
    caption(cap)

def read_csv(name):
    rows = []
    with open(os.path.join(RESULTS, name), encoding="utf-8-sig") as f:
        for row in csv.reader(f, delimiter=";"):
            rows.append(row)
    return rows

def results_table(csv_name, cap, work_header):
    rows = read_csv(csv_name)
    t = doc.add_table(rows=1, cols=4)
    t.style = "Table Grid"
    t.alignment = WD_TABLE_ALIGNMENT.CENTER
    hdr = t.rows[0].cells
    for i, txt in enumerate(["n", "Лучшее время, с", "Среднее время, с", work_header]):
        hdr[i].paragraphs[0].add_run(txt).bold = True
    for row in rows[1:]:
        cells = t.add_row().cells
        n, best, avg, work = row
        cells[0].text, cells[1].text, cells[2].text, cells[3].text = n, best, avg, f"{int(work):,}".replace(",", " ")
    caption(cap)
    return t

# ============================ ТИТУЛЬНЫЙ ЛИСТ ============================
for _ in range(2):
    doc.add_paragraph()
body("Челябинский Государственный Университет", align=WD_ALIGN_PARAGRAPH.CENTER, indent=False, size=12)
body("Кафедра Институт Информационных Технологий", align=WD_ALIGN_PARAGRAPH.CENTER, indent=False, size=12)
for _ in range(4):
    doc.add_paragraph()
body("ЛАБОРАТОРНАЯ РАБОТА №2", bold=True, align=WD_ALIGN_PARAGRAPH.CENTER, indent=False, size=16)
body("по дисциплине «Алгоритмы и структуры данных»", align=WD_ALIGN_PARAGRAPH.CENTER, indent=False)
body("Тема: «Рекурсивные алгоритмы»", bold=True, align=WD_ALIGN_PARAGRAPH.CENTER, indent=False)
for _ in range(4):
    doc.add_paragraph()
body("Выполнил: студент группы ПрИ-202", align=WD_ALIGN_PARAGRAPH.RIGHT, indent=False)
body("________________________", align=WD_ALIGN_PARAGRAPH.RIGHT, indent=False)
for _ in range(6):
    doc.add_paragraph()
body("Челябинск — 2026", align=WD_ALIGN_PARAGRAPH.CENTER, indent=False)
doc.add_page_break()

# ============================ СОДЕРЖАНИЕ ============================
doc.add_heading("Содержание", level=1)
for line in [
    "Введение",
    "1. Описание архитектуры приложения",
    "2. Описание работы приложения",
    "3. Результаты исследования алгоритмов",
    "3.1. Ханойские башни",
    "3.2. Задача о N ферзях",
    "3.3. Быстрая сортировка",
    "3.4. Треугольник Серпинского",
    "4. Выводы",
    "5. Список использованных источников",
]:
    body(line, indent=False)

# ============================ ВВЕДЕНИЕ ============================
doc.add_heading("Введение", level=1)
body("Цель данной лабораторной работы — изучение рекурсивных алгоритмов: их реализации, "
     "эмпирическое исследование временной сложности и наглядная демонстрация работы.")
body("Задачи:", bold=True, indent=False)
for t in [
    "- Реализовать рекурсивные алгоритмы различной асимптотической сложности: O(2^n), O(N!), O(n·log n), O(3^d)",
    "- Разработать измерительный стенд с прогревочными прогонами и многократным повторением замеров",
    "- Провести серию экспериментов и построить графики зависимости времени выполнения от размера задачи",
    "- Сравнить теоретические и эмпирические оценки сложности",
    "- Разработать интерактивные демонстрации работы алгоритмов с пошаговой анимацией",
    "- Разработать демонстрацию рекурсивного построения фрактала (треугольник Серпинского)",
]:
    body(t, indent=False)

# ============================ 1. АРХИТЕКТУРА ============================
doc.add_heading("1. Описание архитектуры приложения", level=1)
doc.add_heading("1.1. Стек технологий", level=2)
t = doc.add_table(rows=1, cols=3)
t.style = "Table Grid"
for i, txt in enumerate(["Компонент", "Технология", "Назначение"]):
    t.rows[0].cells[i].paragraphs[0].add_run(txt).bold = True
for row in [
    ("Язык программирования", "C# 13 / .NET 10", "Реализация алгоритмов и приложений"),
    ("Алгоритмическое ядро", "RecursionLab2.Core", "Рекурсивные алгоритмы, стенд замеров, отрисовка графиков — без GUI"),
    ("GUI-приложение", "Windows Forms", "Интерактивные демонстрации алгоритмов и встроенный стенд замеров"),
    ("Консольный стенд", "RecursionLab2.Bench", "Серии замеров, экспорт CSV и PNG-графиков"),
    ("Тесты", "xUnit", "52 модульных теста корректности алгоритмов"),
    ("Замер времени", "System.Diagnostics.Stopwatch", "Высокоточное измерение времени выполнения"),
    ("Графики", "System.Drawing", "Отрисовка графиков времени в PNG и на панели GUI"),
]:
    c = t.add_row().cells
    c[0].text, c[1].text, c[2].text = row

doc.add_heading("1.2. Структура решения", level=2)
body("Решение разделено на четыре проекта по назначению. Ключевой принцип: алгоритмы "
     "полностью изолированы от интерфейса — библиотека Core не содержит GUI-кода. Благодаря "
     "этому один и тот же код используется консольным стендом, GUI-приложением и тестами, "
     "а замеры времени остаются честными: в измеряемый код не вмешивается отрисовка.")
code([
    "RecursionLab2.sln",
    "├── src/RecursionLab2.Core      # алгоритмы, замер, графики (без GUI)",
    "│   ├── Hanoi/HanoiSolver.cs",
    "│   ├── NQueens/NQueensSolver.cs",
    "│   ├── QuickSort/QuickSorter.cs",
    "│   ├── Sierpinski/SierpinskiTriangle.cs",
    "│   ├── Benchmark/BenchmarkRunner.cs   # стенд замеров",
    "│   └── Charting/ChartPainter.cs       # графики времени",
    "├── src/RecursionLab2.App       # WinForms GUI: 4 демонстрации + стенд замеров",
    "├── src/RecursionLab2.Bench     # консольный стенд: CSV + PNG в results/",
    "└── tests/RecursionLab2.Tests   # 52 модульных теста (xUnit)",
])

doc.add_heading("1.3. Стенд замеров", level=2)
body("Замеры выполняются классом BenchmarkRunner по методике лабораторной работы №1:")
for t in [
    "1) прогревочный прогон — компиляция JIT и прогрев кэшей процессора;",
    "2) три рабочих прогона, время каждого измеряется Stopwatch;",
    "3) фиксируются лучшее время (минимальный прогон, наименее искажённый сторонними процессами) и среднее время (используется на графиках);",
    "4) дополнительно фиксируется теоретический объём работы (число переносов, решений, треугольников, оценка n·log₂n).",
]:
    body(t, indent=False)

# ============================ 2. РАБОТА ПРИЛОЖЕНИЯ ============================
doc.add_heading("2. Описание работы приложения", level=1)
body("Руководство пользователя:", bold=True, indent=False)
for t in [
    "1) Проверить, что установлен .NET 10 SDK (dotnet --version).",
    "2) Собрать решение: dotnet build.",
    "3) Запустить GUI-приложение: dotnet run --project src/RecursionLab2.App — откроется окно с пятью вкладками.",
    "4) Для замеров из консоли: dotnet run --project src/RecursionLab2.Bench — результаты сохраняются в results/ (CSV и PNG-графики).",
]:
    body(t, indent=False)
body("Вкладка «Ханойские башни»: выбор числа дисков (1–12), пуск/пауза/сброс, регулировка "
     "скорости. Диски анимированно переносятся по рекурсивному решению; отображаются журнал "
     "ходов и счётчики «выполнено/всего» (2^n − 1).")
body("Вкладка «Задача о N ферзях»: пошаговая анимация бэктрекинга — ферзь ставится, "
     "атакованные клетки подсвечиваются, при тупике ферзь снимается. Предусмотрены пошаговый "
     "режим «Шаг», поиск первого или всех решений, счётчики попыток/откатов/решений. Все "
     "найденные решения накапливаются и листаются кнопками «Пред./След. решение».")
body("Вкладка «Быстрая сортировка»: массив рисуется столбиками; опорный элемент подсвечен "
     "зелёным, текущее сравнение — оранжевым; видны разбиение вокруг опорного и рекурсивный "
     "спуск. Регулировка скорости, пауза, генерация нового массива, счётчики сравнений и обменов.")
body("Вкладка «Треугольник Серпинского»: ползунок глубины рекурсии 0–9 (число треугольников "
     "3^d и время построения), автоматическая анимация роста глубины, экспорт в PNG.")
body("Вкладка «Замеры времени»: запуск серии замеров прямо из GUI, таблица результатов, "
     "график с логарифмической/линейной шкалой, сохранение CSV и PNG.")

# ============================ 3. РЕЗУЛЬТАТЫ ============================
doc.add_heading("3. Результаты исследования алгоритмов", level=1)

# ---------- 3.1 Ханой ----------
doc.add_heading("3.1. Ханойские башни", level=2)
body("Задача: перенести башню из n дисков с первого стержня на третий, за раз перенося один "
     "диск и не кладя больший на меньший (головоломка Э. Люка, 1883).")
body("Рекурсивная идея: чтобы перенести n дисков со стержня A на C, нужно (1) перенести "
     "n−1 дисков с A на вспомогательный B, (2) перенести самый большой диск с A на C, "
     "(3) перенести n−1 дисков с B на C. База рекурсии: n = 0.")
body("Код рекурсии (RecursionLab2.Core/Hanoi/HanoiSolver.cs):", bold=True, indent=False)
code([
    "private static void Move(int n, int from, int to, int aux, List<HanoiMove> acc)",
    "{",
    "    if (n == 0) return;                  // база рекурсии",
    "    Move(n - 1, from, aux, to, acc);     // шаг 1: убрать башню на вспомогательный",
    "    acc.Add(new HanoiMove(n, from, to)); // шаг 2: перенести самый большой диск",
    "    Move(n - 1, aux, to, from, acc);     // шаг 3: вернуть башню поверх него",
    "}",
])
body("O-нотация:", bold=True, indent=False)
body("Число переносов удовлетворяет рекуррентному соотношению T(n) = 2·T(n−1) + 1, откуда "
     "T(n) = 2^n − 1 — минимально возможное число ходов. Временная сложность O(2^n), глубина "
     "рекурсии O(n), дополнительная память O(n).")
body("Результаты замеров (n = 10…29):", bold=True, indent=False)
results_table("hanoi.csv", "Табл. 1 — Замеры времени: Ханойские башни", "Число переносов")
figure("hanoi.png", "Рис. 1 — График времени работы: Ханойские башни")
body("Вывод: время удваивается при каждом увеличении n на единицу (0.033 → 0.062 → 0.125 → "
     "0.246 … с), что точно соответствует теоретической сложности O(2^n).")

# ---------- 3.2 Ферзи ----------
doc.add_heading("3.2. Задача о N ферзях", level=2)
body("Задача: расставить на доске N×N N ферзей так, чтобы никакие два не били друг друга "
     "(не стояли на одной вертикали, горизонтали или диагонали).")
body("Рекурсивная идея (бэктрекинг): ферзи расставляются по одному на строку сверху вниз. "
     "В строке перебираются столбцы; клетка безопасна, если свободны её столбец и обе "
     "диагонали. Ферзь ставится, и рекурсивно решается задача для следующей строки; если "
     "ветвь заходит в тупик — ферзь снимается и перебор продолжается (возврат). База "
     "рекурсии: строка вышла за пределы доски — решение найдено.")
body("Код рекурсии (RecursionLab2.Core/NQueens/NQueensSolver.cs):", bold=True, indent=False)
code([
    "private long PlaceRow(int row, int n, bool stopAtFirst, CancellationToken ct)",
    "{",
    "    if (row == n) return 1;                       // база: доска заполнена",
    "    long found = 0;",
    "    for (int col = 0; col < n; col++)",
    "    {",
    "        if (_cols[col] || _diag1[row + col] || _diag2[row - col + n - 1])",
    "            continue;                             // клетка под боем",
    "        _cols[col] = _diag1[row + col] = _diag2[row - col + n - 1] = true;",
    "        found += PlaceRow(row + 1, n, stopAtFirst, ct);",
    "        _cols[col] = _diag1[row + col] = _diag2[row - col + n - 1] = false; // откат",
    "    }",
    "    return found;",
    "}",
])
body("Техническая деталь: занятость столбцов и диагоналей хранится в трёх булевых массивах, "
     "поэтому проверка «под боем» выполняется за O(1), а не за O(N).")
body("O-нотация:", bold=True, indent=False)
body("Худший случай O(N!) — проверки на каждом уровне усекают дерево перебора. Глубина "
     "рекурсии O(N), дополнительная память O(N).")
body("Результаты замеров (подсчёт всех решений, n = 4…15):", bold=True, indent=False)
results_table("nqueens.csv", "Табл. 2 — Замеры времени: задача о N ферзях", "Число решений")
figure("nqueens.png", "Рис. 2 — График времени работы: задача о N ферзях")
body("Корректность подтверждается совпадением числа найденных решений с известными "
     "математическими значениями: 2 (n=4), 10 (n=5), 92 (n=8), 724 (n=10), 14200 (n=12), "
     "365596 (n=14), 2279184 (n=15).")
body("Вывод: при увеличении n на единицу время умножается примерно на 5–6 (0.56 с при n=13 → "
     "3.43 с при n=14 → 22.3 с при n=15) — рост быстрее экспоненциального, что соответствует "
     "факториальной сложности перебора с возвратом.")

# ---------- 3.3 QuickSort ----------
doc.add_heading("3.3. Быстрая сортировка", level=2)
body("Задача: отсортировать массив чисел по возрастанию. Быстрая сортировка (QuickSort, "
     "Ч. Хоар, 1960) — один из самых используемых алгоритмов сортировки (основа Array.Sort "
     "в .NET и std::sort в C++).")
body("Рекурсивная идея (разделяй и властвуй): выбрать опорный элемент (в реализации — "
     "медиана из первого, среднего и последнего элементов части); разбиением по схеме "
     "Ломуто собрать элементы ≤ опорного слева, остальные — справа, опорный встаёт на "
     "финальное место; затем рекурсивно отсортировать обе части. База рекурсии: 0 или 1 "
     "элемент в части.")
body("Код рекурсии (RecursionLab2.Core/QuickSort/QuickSorter.cs):", bold=True, indent=False)
code([
    "private void Sort(int lo, int hi)",
    "{",
    "    if (lo >= hi) return;          // база рекурсии",
    "    int p = Partition(lo, hi);     // опорный встал на место p",
    "    Sort(lo, p - 1);               // левая часть: всё <= опорного",
    "    Sort(p + 1, hi);               // правая часть: всё > опорного",
    "}",
])
body("O-нотация:", bold=True, indent=False)
body("Средний случай O(n·log n): в среднем разбиение делит часть пополам, поэтому уровней "
     "рекурсии log₂n, и на каждом уровне в сумме просматривается n элементов. Худший случай "
     "O(n²) предотвращается выбором опорного медианой из трёх (проверено тестом на "
     "упорядоченном массиве из 200 000 элементов). Сортировка выполняется на месте, "
     "дополнительная память — O(log n) на стек рекурсии. Сортировка неустойчивая.")
body("Результаты замеров (случайный массив, n от 2^10 до 2^24):", bold=True, indent=False)
results_table("quicksort.csv", "Табл. 3 — Замеры времени: быстрая сортировка", "Оценка сравнений n·log₂n")
figure("quicksort.png", "Рис. 3 — График времени работы: быстрая сортировка")
body("Вывод: при росте размера массива в 16 384 раза (от 1 024 до 16 777 216 элементов) время "
     "выросло с 0.0002 до 8.44 с — примерно в n·log n раз, а не в n раз. График — плавная, "
     "почти линейная кривая, характерная для O(n·log n).")

# ---------- 3.4 Серпинский ----------
doc.add_heading("3.4. Треугольник Серпинского", level=2)
body("Треугольник Серпинского — самоподобный фрактал (В. Серпинский, 1915). Треугольник "
     "делится отрезками между серединами сторон на 4 равных; центральный выбрасывается, и к "
     "трём оставшимся применяется та же процедура. База рекурсии: глубина 0 — рисуется сам "
     "треугольник.")
body("Код рекурсии (RecursionLab2.Core/Sierpinski/SierpinskiTriangle.cs):", bold=True, indent=False)
code([
    "private static void Divide(float ax, float ay, float bx, float by,",
    "                           float cx, float cy, int depth, List<TriangleF> acc)",
    "{",
    "    if (depth == 0) { acc.Add(new TriangleF(ax, ay, bx, by, cx, cy)); return; }",
    "    float abX = (ax + bx) / 2, abY = (ay + by) / 2;  // середины сторон",
    "    float bcX = (bx + cx) / 2, bcY = (by + cy) / 2;",
    "    float caX = (cx + ax) / 2, caY = (cy + ay) / 2;",
    "    Divide(ax, ay, abX, abY, caX, caY, depth - 1, acc);  // верхний",
    "    Divide(abX, abY, bx, by, bcX, bcY, depth - 1, acc);  // левый",
    "    Divide(caX, caY, bcX, bcY, cx, cy, depth - 1, acc);  // правый",
    "}",
])
body("O-нотация:", bold=True, indent=False)
body("На глубине d получается 3^d треугольников; сложность построения O(3^d). Суммарная "
     "площадь закрашенных треугольников равна (3/4)^d от исходной и стремится к нулю, при "
     "этом периметр растёт неограниченно — фрактальная размерность log₂3 ≈ 1.585. Память — "
     "O(d) на глубину стека рекурсии.")
body("Результаты замеров (рекурсивный обход без хранения, глубина 0…19):", bold=True, indent=False)
results_table("sierpinski.csv", "Табл. 4 — Замеры времени: треугольник Серпинского", "Число треугольников")
figure("sierpinski.png", "Рис. 4 — График времени работы: треугольник Серпинского")
body("Вывод: время растёт ровно в 3 раза на каждую единицу глубины (0.22 → 0.62 → 1.87 → "
     "5.52 с на глубинах 16–19), что соответствует O(3^d); на глубине 19 выполняется "
     "1 162 261 467 рекурсивных вызовов.")

# ============================ 4. ВЫВОДЫ ============================
doc.add_heading("4. Выводы", level=1)
doc.add_heading("4.1. Подтверждение теоретической сложности", level=2)
body("Вывод: теоретическая сложность всех четырёх алгоритмов подтвердилась эмпирически. "
     "Ханойские башни — удвоение времени при +1 к n (O(2^n)); задача о N ферзях — рост "
     "времени в 5–6 раз при +1 к n (близко к O(N!)); быстрая сортировка — почти линейный "
     "рост на массиве от 1 тыс. до 16.7 млн элементов (O(n·log n)); треугольник Серпинского — "
     "утроение времени на единицу глубины (O(3^d)).")
doc.add_heading("4.2. Сопоставление алгоритмов", level=2)
body("Наглядно видно различие классов сложности: быстрая сортировка обрабатывает 16.7 млн "
     "элементов за 8.4 с, тогда как экспоненциальные алгоритмы достигают сопоставимого "
     "времени уже на «маленьких» входах — Ханойские башни при n = 29 (полмиллиарда "
     "переносов), задача о ферзях при n = 15. Это демонстрирует принципиальную разницу между "
     "полиномиальными и экспоненциальными алгоритмами.")
doc.add_heading("4.3. Тестирование", level=2)
body("Корректность алгоритмов подтверждена 52 модульными тестами (xUnit): число ходов "
     "Ханойских башен равно 2^n − 1 и все ходы легальны; число решений задачи о ферзях "
     "совпадает с известными значениями; результат быстрой сортировки совпадает с эталонным "
     "Array.Sort на случайных, упорядоченных, обратно упорядоченных и константных массивах; "
     "число треугольников Серпинского равно 3^d, а суммарная площадь — (3/4)^d от исходной.")

# ============================ 5. ИСТОЧНИКИ ============================
doc.add_heading("5. Список использованных источников", level=1)
for i, s in enumerate([
    "Документация Microsoft .NET: https://learn.microsoft.com/dotnet/",
    "Документация C#: https://learn.microsoft.com/dotnet/csharp/",
    "Документация Windows Forms: https://learn.microsoft.com/dotnet/desktop/winforms/",
    "Т. Кормен, Ч. Лейзерсон, Р. Ривест, К. Штайн. Алгоритмы: построение и анализ. — М.: Вильямс, 2013.",
    "Р. Седжвик, К. Уэйн. Алгоритмы на Java. — СПб.: Диалектика, 2023.",
    "Mandelbrot B. B. The Fractal Geometry of Nature. — W. H. Freeman, 1982.",
    "xUnit.net: https://xunit.net/",
], start=1):
    body(f"{i}. {s}", indent=False)

out = os.path.join(ROOT, "Otchet-Laba2.docx")
doc.save(out)
print("Сохранено:", out)
