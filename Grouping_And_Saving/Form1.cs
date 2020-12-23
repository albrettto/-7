using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Grouping_And_Saving
{
    public partial class Form1 : Form
    {
        MVC model;
        public Form1()
        {
            InitializeComponent();
            model = new MVC();
            model.observers += new System.EventHandler(this.updateFromMVC);
        }
        class Shape // Родительский (класс) фигуры
        {
            private Color color = default_color;
            private bool is_selected = false;
            public int move = 60; // смещение от цетра к вершинам
            public Shape()
            {
            }
            public void SetColor(Color color)
            {
                this.color = color;
            }
            public Color GetColor()
            {
                return color;
            }
            public void Select(bool is_selected)
            {
                this.is_selected = is_selected;
            }
            public bool IsSelected()
            {
                return is_selected;
            }
        };
        class Circle : Shape // Класс круга
        {
            public int x, y;
            public Circle() // Конструктор по умолчанию
            {
                x = 0;
                y = 0;
            }
            public Circle(int x, int y) // Конструктор с параметрами
            {
                this.x = x - move / 2;
                this.y = y - move / 2;
            }
        }
        class Triangle : Shape // Класс треугольника
        {
            public int x1, y1, x2, y2, x3, y3;
            public int st_move = 60;
            public Triangle() // Конструктор по умолчанию
            {
                x1 = 0; y1 = 0;
                x2 = 0; y2 = 0;
                x3 = 0; y3 = 0;
            }
            public Triangle(int x, int y) // Конструктор с параметрами
            {
                x1 = x; y1 = y - move * 2 / 3;
                x2 = x - move * 2 / 3; y2 = y + move / 3;
                x3 = x + move * 2 / 3; y3 = y + move / 3;
            }
        }
        class Square : Shape // Класс квадрата
        {
            public int x1, y1, x2, y2, x3, y3, x4, y4;
            public int st_move = 60;
            public Square() // Конструктор по умолчанию
            {
                x1 = 0; y1 = 0;
                x2 = 0; y2 = 0;
                x3 = 0; y3 = 0;
                x4 = 0; y4 = 0;

            }
            public Square(int x, int y) // Конструктор с параметрами
            {
                x1 = x - move / 2; y1 = y + move / 2;
                x2 = x + move / 2; y2 = y + move / 2;
                x3 = x + move / 2; y3 = y - move / 2;
                x4 = x - move / 2; y4 = y - move / 2;
            }
        }
        class Storage
        {
            public Shape[] objects;
            public Storage(int amount)
            {   // Конструктор по умолчанию 
                objects = new Shape[amount];
                for (int i = 0; i < amount; ++i)
                    objects[i] = null;
            }
            public void Initialization(int amount)
            {   // Выделяем amount мест в хранилище
                objects = new Shape[amount];
                for (int i = 0; i < amount; ++i)
                    objects[i] = null;
            }
            public void Add_object(int ind, ref Shape new_object, int count, ref int indexin)
            {   // Добавляет ячейку в хранилище
                // Если ячейка занята ищем свободное место
                while (objects[ind] != null)
                {
                    ind = (ind + 1) % count;
                }
                objects[ind] = new_object;
                indexin = ind;
            }
            public void Delete_object(ref int count_elements)
            {   // Удаляет объект из хранилища
                if (objects[count_elements] != null)
                {
                    objects[count_elements] = null;
                    count_elements--;
                }
            }
            public bool Is_empty(int count_elements)
            {   // Проверяет занято ли место в хранилище
                if (objects[count_elements] == null)
                    return true;
                else return false;
            }
            public int Occupied(int size)
            { // Определяет кол-во занятых мест в хранилище
                int count_occupied = 0;
                for (int i = 0; i < size; ++i)
                    if (!Is_empty(i))
                        ++count_occupied;
                return count_occupied;
            }
            public void Increase_Storage(ref int size)
            { // Увеличивает хранилище в 2 раза
                Storage new_storage = new Storage(size * 2);
                for (int i = 0; i < size; ++i)
                    new_storage.objects[i] = objects[i];
                for (int i = size; i < (size * 2) - 1; ++i)
                {
                    new_storage.objects[i] = null;
                }
                size *= 2;
                Initialization(size);
                for (int i = 0; i < size; ++i)
                    objects[i] = new_storage.objects[i];
            }
            ~Storage() { }
        };
        public class MVC
        {
            private string figure;
            public System.EventHandler observers;
            public void setFigure(string figure)
            {

                this.figure = figure;
                observers.Invoke(this, null);
            }
            public string getFigure()
            {
                return figure;
            }
        }
        #region declaring variables
        string figure_now; // Хранит значение нынешней фигуры
        static int count_cells = 5; // Кол-во ячеек в хранилище
        int indexin = 0; // Индекс, в какое место был помещён круг
        int count_elements = 0; // Кол-во элементов в хранилище
        Storage storage = new Storage(count_cells); // Хранилище объектов
        Point[] points = new Point[3]; // Массив точек для прорисовки треугольника
        static Color default_color = Color.Black; // Цвет по умолчанию
        Pen pen = new Pen(default_color, 3); // Ручка для рисования
        Shape shape = new Shape(); // Объект класса Shape
        SolidBrush solidBrush = new SolidBrush(Color.LightGray); // Цвет для заливки
        #endregion
        private void Canvas_Panel_MouseDown(object sender, MouseEventArgs e)//Обработчик нажатия на полотно
        {
            int ind = is_free(ref storage, e.X, e.Y, 0);
            if (e.Button == MouseButtons.Left)
            {
                if (count_elements == count_cells)
                    // Увеличиваем хранилище
                    storage.Increase_Storage(ref count_cells);

                switch (figure_now)//Узнаем какая сейчас выбрана фигура
                {
                    case "Круг":
                        shape = new Circle(e.X, e.Y);
                        break;
                    case "Треугольник":
                        shape = new Triangle(e.X, e.Y);
                        break;
                    case "Квадрат":
                        shape = new Square(e.X, e.Y);
                        break;
                }
                if (ind == -1)//Если место свободно
                {
                    deselect(ref storage);//Убираем выделение у всех объектов
                    redrawing_figures(ref storage);//Перерисовываем все объекты
                    storage.Add_object(indexin, ref shape, count_cells, ref indexin);//Добавляем объект в хранилище
                    shape.Select(true);//Объект выделяем
                    draw_figure(shape);//Рисуем объект
                    shape.Select(false);//Снимаем выделение
                    count_elements++;//Увеличиваем кол-во элементов
                }
            }
            if (e.Button == MouseButtons.Right)
            {
                if (ind != -1)//Если там есть фигуры
                {
                    if (Control.ModifierKeys != Keys.Control)//Если не зажат Ctrl
                    {
                        deselect(ref storage);//Очищаем панель
                        redrawing_figures(ref storage);//Перерисовываем фигуры
                    }
                    storage.objects[ind].Select(true);//Выделяем объект
                    int start = ind;
                    while (start != count_cells - 1)//Проверяем нет ли на этом мечет еще объектов
                    {
                        if (is_free(ref storage, e.X, e.Y, start + 1) != -1)
                        {
                            storage.objects[is_free(ref storage, e.X, e.Y, start + 1)].Select(true);
                            start = is_free(ref storage, e.X, e.Y, start + 1);
                        }
                        else
                            break;
                    }
                    redrawing_figures(ref storage);//Перерисовываем фигуры
                }

            }

        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)//Обработчик нажатия на клавиши
        {
            if (e.KeyCode == Keys.Delete)//Если нажили на Delete
            {
                for (int i = 0; i < count_cells; ++i)//Удаляем выделенные объекты
                    if (!storage.Is_empty(i))
                        if (storage.objects[i].IsSelected())
                        {
                            storage.Delete_object(ref i);
                            count_elements--;
                        }
                redrawing_figures(ref storage);
            }
            if (e.KeyCode == Keys.Add)//Если +
                Plus_toolStripButton_Click(sender, e);
            if (e.KeyCode == Keys.Subtract)//Если -
                Minus_toolStripButton_Click(sender, e);
            switch (e.KeyCode)//Если одна из стрелок
            {
                case Keys.Up:
                    move_y(-5);
                    redrawing_figures(ref storage);
                    break;
                case Keys.Down:
                    move_y(5);
                    redrawing_figures(ref storage);
                    break;
                case Keys.Right:
                    move_x(5);
                    redrawing_figures(ref storage);
                    break;
                case Keys.Left:
                    move_x(-5);
                    redrawing_figures(ref storage);
                    break;
            }
        }
        private void Circle_ToolStripButton_Click(object sender, EventArgs e)
        {
            figure_now = "Круг";
            model.setFigure(figure_now);
        }
        private void Triangle_ToolStripButton_Click(object sender, EventArgs e)
        {
            figure_now = "Треугольник";
            model.setFigure(figure_now);
        }
        private void Square_ToolStripButton_Click(object sender, EventArgs e)
        {
            figure_now = "Квадрат";
            model.setFigure(figure_now);
        }
        private void Color_ToolStripButton_Click(object sender, EventArgs e)
        {
            if (ColorDialog.ShowDialog() == DialogResult.Cancel)
                return;
            for (int i = 0; i < count_cells; ++i)
                if(!storage.Is_empty(i))
                    if (storage.objects[i].IsSelected())
                    {
                        storage.objects[i].SetColor(ColorDialog.Color);
                        redrawing_figures(ref storage);
                    }
        }
        private void Plus_toolStripButton_Click(object sender, EventArgs e)//Увеличиваем размер фигуры
        {
            for (int i = 0; i < count_cells; ++i)
                if (!storage.Is_empty(i))
                    if (storage.objects[i].IsSelected())
                        storage.objects[i].move += 5;
            redrawing_figures(ref storage);
        }
        private void Minus_toolStripButton_Click(object sender, EventArgs e)//Умемньшаем размер фигуры
        {
            for (int i = 0; i < count_cells; ++i)
                if (!storage.Is_empty(i))
                    if (storage.objects[i].IsSelected())
                        storage.objects[i].move -= 5;
            redrawing_figures(ref storage);
        }
        
        private void Group_ToolStripButton_Click(object sender, EventArgs e)
        {

        }
        private void Write_ToolStripButton_Click(object sender, EventArgs e)
        {
            // создаем каталог для файла
            string path = @"C:\Users\User\OneDrive\Рабочий стол\УНИВЕР\2 КУРС\3 СЕМЕСТР\ООП\Лабораторная работа 7";
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
                dirInfo.Create();
            

            // запись в файл
            using (FileStream fstream = new FileStream($@"{path}\figures.txt", FileMode.OpenOrCreate))
            {
                string text = count_elements.ToString();
                // преобразуем строку в байты
                byte[] array = System.Text.Encoding.Default.GetBytes(text + "\n");
                fstream.Write(array, 0, array.Length);
                for (int i = 0; i < count_cells; ++i)
                {
                    if (!storage.Is_empty(i))
                    {
                        text = storage.objects[i].move.ToString();
                        array = System.Text.Encoding.Default.GetBytes(text + "\n");
                        // запись массива байтов в файл
                        fstream.Write(array, 0, array.Length);
                    }
                }
            }
        }
        private void Read_ToolStripButton_Click(object sender, EventArgs e)
        {

        }
        private void Clear_toolStripButton_Click(object sender, EventArgs e)
        {
            Canvas_Panel.Refresh();//Обновляем панель
            for (int i = 0; i < count_cells; ++i)
                storage.Delete_object(ref i); // Удаляем объект из хранилища
            count_elements = 0; // Кол-во элементов в хранилище обнуляем 
        }
        int is_free(ref Storage storage, int X, int Y, int start)//Проверка на "свободность" места на полотне
        {
            for (int i = start; i < count_cells; ++i)
            {//Проходимся по всему хранилищу
                if (storage.Is_empty(i))
                    continue;
                if ((storage.objects[i] as Circle) != null)//Если это круг
                {
                    Circle c = storage.objects[i] as Circle;
                    if (((X - c.x - c.move / 2) * (X - c.x - c.move / 2) + (Y - c.y - c.move / 2) * (Y - c.y - c.move / 2)) < (c.move / 2) * (c.move / 2))
                        return i;
                }
                if ((storage.objects[i] as Triangle) != null)//Если треугольник
                {
                    Triangle t = storage.objects[i] as Triangle;
                    int a = (t.x1 - X) * (t.y2 - t.y1) - (t.x2 - t.x1) * (t.y1 - Y);
                    int b = (t.x2 - X) * (t.y3 - t.y2) - (t.x3 - t.x2) * (t.y2 - Y);
                    int c = (t.x3 - X) * (t.y1 - t.y3) - (t.x1 - t.x3) * (t.y3 - Y);
                    if (((a > 0 && b > 0 && c > 0) || (a < 0 && b < 0 && c < 0)))
                        return i;
                }
                if ((storage.objects[i] as Square) != null)//Если квадрат
                {
                    Square s = storage.objects[i] as Square;
                    if (X > s.x1 && Y < s.y1 && X < s.x3 && Y > s.y3)
                        return i;
                }
            }
            return -1; // Возвращает -1, если место свободно
        }
        void draw_figure(Shape _object)//Отрисовка объекта 
        {
            if (_object == null)
                return;
            else
                pen.Color = _object.GetColor();
            if ((_object as Circle) != null)//Если это круг
            {
                Circle c = _object as Circle;
                Canvas_Panel.CreateGraphics().DrawEllipse(pen,
                                              c.x,
                                              c.y,
                                              c.move,
                                              c.move);
                if (_object.IsSelected())
                    Canvas_Panel.CreateGraphics().FillEllipse(solidBrush,
                                              c.x,
                                              c.y,
                                              c.move,
                                              c.move);
            }
            if ((_object as Triangle) != null)//Если это треугольник
            {
                Triangle t = _object as Triangle;
                int dif = t.move - t.st_move;
                points[0].X = t.x1; points[0].Y = t.y1 - dif;
                points[1].X = t.x2 - dif; points[1].Y = t.y2 + dif;
                points[2].X = t.x3 + dif; points[2].Y = t.y3 + dif;
                Canvas_Panel.CreateGraphics().DrawPolygon(pen, points);
                if (_object.IsSelected())
                    Canvas_Panel.CreateGraphics().FillPolygon(solidBrush, points);
            }
            if ((_object as Square) != null)//Если это квадрат
            {
                Square s = _object as Square;
                Canvas_Panel.CreateGraphics().DrawRectangle(pen, s.x4, s.y4, s.move, s.move);
                if (_object.IsSelected())
                    Canvas_Panel.CreateGraphics().FillRectangle(solidBrush, s.x4, s.y4, s.move, s.move);
            }
        }
        void redrawing_figures(ref Storage storage)
        {
            Canvas_Panel.Refresh();
            for (int i = 0; i < count_cells; ++i)
                draw_figure(storage.objects[i]);
        }
        void deselect(ref Storage storage)//Отменяет выделение у всех объектов
        {
            for (int i = 0; i < count_cells; ++i)
                if (!storage.Is_empty(i))
                    if (storage.objects[i].IsSelected())
                        storage.objects[i].Select(false);
        }
        void move_x(int move)//Смещение по X
        {
            for (int i = 0; i < count_cells; ++i)
                if (!storage.Is_empty(i))
                    if (storage.objects[i].IsSelected())
                    {
                        if ((storage.objects[i] as Circle) != null)//Если круг
                        {
                            Circle c = storage.objects[i] as Circle;
                            c.x += move;
                            if (!can_go(c.x + c.move, c.y) || !can_go(c.x, c.y))
                                c.x -= move;
                        }
                        if ((storage.objects[i] as Triangle) != null)//Если треугольник
                        {
                            Triangle t = storage.objects[i] as Triangle;
                            t.x1 += move;
                            t.x2 += move;
                            t.x3 += move;
                            if (!can_go(t.x2 - (t.move - t.st_move), t.y2) || !can_go(t.x3 + (t.move - t.st_move), t.y3))
                            {
                                t.x1 -= move;
                                t.x2 -= move;
                                t.x3 -= move;
                            }
                        }
                        if ((storage.objects[i] as Square) != null)//Если квадрат
                        {
                            Square s = storage.objects[i] as Square;
                            s.x1 += move;
                            s.x2 += move;
                            s.x3 += move;
                            s.x4 += move;
                            if (!can_go(s.x1, s.y1) || !can_go(s.x2 + (s.move - s.st_move), s.y2))
                            {
                                s.x1 -= move;
                                s.x2 -= move;
                                s.x3 -= move;
                                s.x4 -= move;
                            }
                        }
                    }
        }
        void move_y(int move)//Смещение по Y
        {
            for (int i = 0; i < count_cells; ++i)
                if (!storage.Is_empty(i))
                    if (storage.objects[i].IsSelected())
                    {
                        if ((storage.objects[i] as Circle) != null)//Если круг
                        {
                            Circle c = storage.objects[i] as Circle;
                            c.y += move;
                            if (!can_go(c.x, c.y + c.move) || !can_go(c.x, c.y))
                                c.y -= move;
                        }
                        if ((storage.objects[i] as Triangle) != null)//Если треугольник
                        {
                            Triangle t = storage.objects[i] as Triangle;
                            t.y1 += move;
                            t.y2 += move;
                            t.y3 += move;
                            if (!can_go(t.x1, t.y1 - (t.move - t.st_move)) || !can_go(t.x3, t.y3 + (t.move - t.st_move)))
                            {
                                t.y1 -= move;
                                t.y2 -= move;
                                t.y3 -= move;
                            }
                        }
                        if ((storage.objects[i] as Square) != null)//Если квадрат
                        {
                            Square s = storage.objects[i] as Square;
                            s.y1 += move;
                            s.y2 += move;
                            s.y3 += move;
                            s.y4 += move;
                            if (!can_go(s.x1, s.y1 + (s.move - s.st_move)) || !can_go(s.x3, s.y3))
                            {
                                s.y1 -= move;
                                s.y2 -= move;
                                s.y3 -= move;
                                s.y4 -= move;
                            }
                        }
                    }
        }
        bool can_go(int X, int Y)//Проверка выхода за границу
        {
            if (X < 0 || X > Canvas_Panel.ClientSize.Width || Y < 0 || Y > Canvas_Panel.ClientSize.Height)
                return false;
            return true;
        }
        void updateFromMVC(object sender, EventArgs e)//Изменение выбранной фигуры и подсветка ее кнопки
        {
            Color back_color = Color.Transparent;
            Color selected_back_color = Color.LightPink;
            Circle_ToolStripButton.BackColor = back_color;
            Triangle_ToolStripButton.BackColor = back_color;
            Square_ToolStripButton.BackColor = back_color;
            switch (model.getFigure())
            {
                case "Круг":
                    Circle_ToolStripButton.BackColor = selected_back_color;
                    break;
                case "Треугольник":
                    Triangle_ToolStripButton.BackColor = selected_back_color;
                    break;
                case "Квадрат":
                    Square_ToolStripButton.BackColor = selected_back_color;
                    break;
            }
        }
    }
}