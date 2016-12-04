using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TaskManager
{
    public partial class MainWindow : Window
    {
        public List<Container> m_containers;
        public int currentIndex;
        public NewTask newTask;
        public NewContainer newContainer;
        public mediametrieEntities bdd;
        public requete req;

        public MainWindow()
        {
            InitializeComponent();
            m_containers = new List<Container>();
            bdd = new mediametrieEntities();
            req = new requete();
            checkTaskDay();
            LoadDB();
            comboBox.SelectedIndex = 0;
            remContainer.IsEnabled = true;
            currentIndex = 0;
            treeView.ItemsSource = m_containers[0].m_Tasks;
        }

        /// <summary>
        /// Gère les tâches à mettre dans le container de la journée
        /// </summary>
        private void checkTaskDay()
        {
            try
            {
                List<taches> actuTaskday = req.getTachesContainer(bdd, "Tâches de la journée");
                List<taches> newTaskDay;

                foreach (taches t in actuTaskday)
                {
                    req.change_container_boite_rec(bdd, t);
                }
                if (req.getNbTaskDay(bdd) > 0)
                {
                    newTaskDay = req.getTachesDay(bdd);
                    foreach (taches t in newTaskDay)
                    {
                        req.change_container_Day(bdd, t);
                        List<taches> newSubTaskDay = req.getSousTaches(bdd, t.label_tache);
                        foreach (taches ta in newSubTaskDay)
                        {
                            req.change_container_Day(bdd, ta);
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Une erreur est survenu au niveau de l'accès aux données: " + "Veuillez réessayer ultérieurement et redémarrer l'application", "Media Task Manager", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadDB()
        {
            try
            {
                List<container> list_containers = new List<container>();
                int index_container = 0;
                list_containers = req.getContainer(bdd);

                foreach(container element in list_containers)
                {
                    int nbTache = req.getNbTacheContainer(bdd, element.label);
                    comboBox.Items.Add(new ComboBoxItem() { Content = element.label });
                    m_containers.Add(new Container() { Name = element.label });

                    List<taches> list_tasks = new List<taches>();
                    int index_task = 0;
                    list_tasks = req.getTachesContainer(bdd, element.label);
                    foreach(taches task in list_tasks)
                    {
                        m_containers[index_container].addItem(null, task.label_tache, task.date_debut.ToString(), task.date_fin.ToString(), task.commentaire, (bool)task.effectuer);
                        m_containers[index_container].m_Tasks[index_task].chkBox.IsEnabled = true;
                        if (task.effectuer == true)
                        {
                            m_containers[index_container].m_Tasks[index_task].chkBox.IsChecked = true;
                        }

                        List<taches> list_subtasks = new List<taches>();
                        list_subtasks = req.getSousTaches(bdd, task.label_tache);
                        foreach(taches subtask in list_subtasks)
                        {
                            m_containers[index_container].addItem(m_containers[index_container].m_Tasks[index_task], subtask.label_tache, subtask.date_debut.ToString(), subtask.date_fin.ToString(), subtask.commentaire, (bool)subtask.effectuer);
                            if (task.effectuer == true)
                            {
                                m_containers[index_container].m_Tasks[index_task].chkBox.IsChecked = true;
                                m_containers[index_container].m_Tasks[index_task].chkBox.IsEnabled = false;
                            }
                        }
                        ++index_task;
                    }
                    ++index_container;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Une erreur est survenu au niveau de l'accès aux données: " + "Veuillez réessayer ultérieurement et redémarrer l'application", "Media Task Manager", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void unselectItem()
        {
            CustomTreeViewItem selected = treeView.SelectedItem as CustomTreeViewItem;
            if (selected != null)
            {
                selected.IsSelected = false;
            }
        }

        private void saveTask_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (Container c in m_containers)
                {
                    if (req.checkContainer(bdd, c.Name) == false)
                        req.ajoutContainer(bdd, new container() { label = c.Name });
                    foreach (CustomTreeViewItem i in c.m_Tasks)
                    {
                        if (i != null)
                        {
                            string parent_task;
                            if (i.parent == null)
                                parent_task = null;
                            else
                                parent_task = i.parent.Name;
                            taches t = new taches() { label_container = c.Name, label_tache = i.Title.Text, label_tache_parent = parent_task, commentaire = i.comment, date_debut = DateTime.Parse(i.dateBegin), date_fin = DateTime.Parse(i.dateEnd), effectuer = i.chkBox.IsChecked };
                           // taches b_t = req.getSTaches(bdd, i.Title.Text);
                            if (req.checkTaches(bdd, t.label_tache) != false)
                                req.modifTaches(bdd, t);
                            else
                                req.ajoutTaches(bdd, t);
                            foreach (CustomTreeViewItem i1 in i.Items)
                            {
                                taches t1 = new taches() { label_container = null, label_tache = i1.Title.Text, label_tache_parent = i.Title.Text, commentaire = i1.comment, date_debut = DateTime.Parse(i1.dateBegin), date_fin = DateTime.Parse(i1.dateEnd), effectuer = i1.chkBox.IsChecked };
                                if (req.checkTaches(bdd, i1.Title.Text) != false)
                                    req.modifTaches(bdd, t1);
                                else
                                    req.ajoutTaches(bdd, t1);
                                
                            }
                        }
                    }
                }
                MessageBox.Show("Les modifications ont bien été ajoutée", "Media Task Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("Une erreur est survenu lors de l'ajout d'une tâche : " + "Veuillez réessayer ultérieurement et redémarrer l'application", "Media Task Manager", MessageBoxButton.OK, MessageBoxImage.Error);
            }
}

        private void newTask_OnSaveNewTask(object sender, RoutedEventArgs e)
        {
            string Name = "Tache";
            int i = 0;
            while (m_containers[currentIndex].m_Tasks.IndexOf(m_containers[currentIndex].m_Tasks.Where(p => p.Title.Text == (Name+i)).FirstOrDefault()) != -1)
                ++i;
            Name += i;
            string content = new TextRange(newTask.comment.Document.ContentStart, newTask.comment.Document.ContentEnd).Text;
            m_containers[currentIndex].addItem(null, Name, newTask.beginDatePicker.Text, newTask.endDatePicker.Text, content, false);
            newTask.Close();
        }

        private void addTask_click(object sender, RoutedEventArgs e)
        {
            newTask = new NewTask();
            newTask.Show();
            newTask.OnSaveEvent += new RoutedEventHandler(newTask_OnSaveNewTask);
        }

        /// <summary>
        /// Ajout une sous-tâches
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newTask_OnSaveNewSubTask(object sender, RoutedEventArgs e)
        {
            try
            { 
                CustomTreeViewItem selected = treeView.SelectedItem as CustomTreeViewItem;
                if (selected != null)
                {
                    if (selected.parent != null)
                        selected = selected.parent;
                    string content = new TextRange(newTask.comment.Document.ContentStart, newTask.comment.Document.ContentEnd).Text;
                    string Name = "Sous-Tache";
                    int i = 0;
                    while (selected.Contains(Name+i) != -1)
                        ++i;
                    Name += i;
                    m_containers[currentIndex].addItem(selected, Name, newTask.beginDatePicker.Text, newTask.endDatePicker.Text, content, false);
                    taches newSubTask = new taches();
                    newSubTask.label_tache = Name;
                    newSubTask.commentaire = content;
                    newSubTask.date_debut = newTask.beginDatePicker.SelectedDate; 
                    newSubTask.date_fin = newTask.endDatePicker.SelectedDate;
                    newSubTask.label_tache_parent = selected.Title.Text;
                    newSubTask.effectuer = false;
                    req.ajoutTaches(bdd, newSubTask);
                    newTask.Close();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Une erreur est survenu lors de l'ajout d'une sous-tâches: " + "Veuillez réessayer ultérieurement et redémarrer l'application", "Media Task Manager", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void addSubTask_click(object sender, RoutedEventArgs e)
        {
            CustomTreeViewItem selected = treeView.SelectedItem as CustomTreeViewItem;
            if (selected != null)
            {
                if (selected.chkBox.IsEnabled == false && (selected.parent == null || (selected.parent != null && selected.parent.chkBox.IsEnabled == false)))
                {
                    MessageBox.Show("Impossible d'ajouter une Sous-Tache!", "Media Task Manager", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                newTask = new NewTask();
                newTask.Show();
                newTask.OnSaveEvent += new RoutedEventHandler(newTask_OnSaveNewSubTask);
            }
        }


        /// Supprime la tache selectionné ainsi que les sous-tâches qui y sont relié
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void remTask_click(object sender, RoutedEventArgs e)
        {
            try
            {
                taches remTask;
                CustomTreeViewItem selected = treeView.SelectedItem as CustomTreeViewItem;

                string nom = treeView.SelectedItem.ToString();
                remTask = req.getSTaches(bdd, selected.Title.Text);

                List<taches> subTaskRem = req.getSousTaches(bdd, selected.Title.Text);
                foreach (taches t in subTaskRem)
                {
                    m_containers[currentIndex].removeItem(selected);
                }
                m_containers[currentIndex].removeItem(selected);
            }
            catch (Exception)
            {
                MessageBox.Show("Une erreur est survenu lors de la suppression: " + "Veuillez réessayer ultérieurement et redémarrer l'application", "Media Task Manager", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void OnSelectTreeViewItem(object sender, RoutedEventArgs e)
        {
            CustomTreeViewItem selected = treeView.SelectedItem as CustomTreeViewItem;
            if (selected != null)
            {
                if (selected.dateBegin != null)
                    beginDatePicker.SelectedDate = DateTime.Parse(selected.dateBegin);
                if (selected.dateEnd != null)
                    endDatePicker.SelectedDate = DateTime.Parse(selected.dateEnd);
                if (selected.comment != null)
                {
                    comment.Document.Blocks.Clear();
                    comment.Document.Blocks.Add(new Paragraph(new Run(selected.comment)));
                }
                statusText.Text = "Taches: " + m_containers[currentIndex].m_Tasks.Count;
                int sTaskCount = 0;
                for (int i = 0; i < m_containers[currentIndex].m_Tasks.Count; ++i)
                    sTaskCount += m_containers[currentIndex].m_Tasks[i].Items.Count;
                statusText.Text += " Sous-Taches: " + sTaskCount;
            }
        }

        private void DatePicker_Loaded(object sender, RoutedEventArgs e)
        {
            DatePicker datePicker = sender as DatePicker;
            if (datePicker != null)
            {
                System.Windows.Controls.Primitives.DatePickerTextBox datePickerTextBox = FindVisualChild<System.Windows.Controls.Primitives.DatePickerTextBox>(datePicker);
                if (datePickerTextBox != null)
                {

                    ContentControl watermark = datePickerTextBox.Template.FindName("PART_Watermark", datePickerTextBox) as ContentControl;
                    if (watermark != null)
                    {
                        watermark.Content = "                ";
                    }
                }
            }
        }

        private T FindVisualChild<T>(DependencyObject depencencyObject) where T : DependencyObject
        {
            if (depencencyObject != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depencencyObject); ++i)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depencencyObject, i);
                    T result = (child as T) ?? FindVisualChild<T>(child);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }

        private void comment_TextInput(object sender, TextCompositionEventArgs e)
        {
            CustomTreeViewItem selected = treeView.SelectedItem as CustomTreeViewItem;
            if (selected != null)
            {
                string content = new TextRange(comment.Document.ContentStart, comment.Document.ContentEnd).Text;
                selected.comment = content;
            }
        }

        private void comment_KeyUp(object sender, KeyEventArgs e)
        {
            CustomTreeViewItem selected = treeView.SelectedItem as CustomTreeViewItem;
            if (selected != null)
            {
                string content = new TextRange(comment.Document.ContentStart, comment.Document.ContentEnd).Text;
                selected.comment = content;
            }
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Load tree for container
            ComboBoxItem item = comboBox.SelectedItem as ComboBoxItem;
            if (item == null)
                return;
            for(int i = 0; i < m_containers.Count; ++i)
            {
                if (m_containers[i].Name == item.Content.ToString())
                {
                    if (i < 3)
                        remContainer.IsEnabled = false;
                    else
                        remContainer.IsEnabled = true;
                    treeView.ItemsSource = m_containers[i].m_Tasks;
                    statusText.Text = "Taches: " + m_containers[i].m_Tasks.Count;
                    int sTaskCount = 0;
                    for (int j = 0; j < m_containers[i].m_Tasks.Count; ++j)
                        sTaskCount += m_containers[i].m_Tasks[j].Items.Count;
                    statusText.Text += " Sous-Taches: " + sTaskCount;
                    if (m_containers[i].m_Tasks.Count > 0)
                        m_containers[i].m_Tasks[0].IsSelected = true;
                    this.currentIndex = i;
                    break;
                }

            }
        }

        private void endDatePicker_CalendarClosed(object sender, RoutedEventArgs e)
        {
            CustomTreeViewItem selected = treeView.SelectedItem as CustomTreeViewItem;
            if (selected != null)
            {
                selected.dateEnd = endDatePicker.Text;
            }
        }

        private void beginDatePicker_CalendarClosed(object sender, RoutedEventArgs e)
        {
            CustomTreeViewItem selected = treeView.SelectedItem as CustomTreeViewItem;
            if (selected != null)
            {
                selected.dateBegin = beginDatePicker.Text;
            }
        }

        private void newContainer_addContainer(object sender, RoutedEventArgs e)
        {
            if (req.checkContainer(bdd, newContainer.collectionName.Text) == true)
                MessageBox.Show("Un container ayant ce nom existe déjà", "Media Task Manager", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            else
            {
                Container container = new Container();
                container.Name = newContainer.collectionName.Text;
                ComboBoxItem cbBoxItem = new ComboBoxItem();
                cbBoxItem.Content = newContainer.collectionName.Text;
                comboBox.Items.Add(cbBoxItem);
                m_containers.Add(container);
                newContainer.Close();
            }
        }
        /*                      Ajout un container                          */
        private void addContainer_Click(object sender, RoutedEventArgs e)
        {
            newContainer = new NewContainer();
            newContainer.Show();
            newContainer.OnAddEvent += new RoutedEventHandler(newContainer_addContainer);
        }

        /// <summary>
        /// Supprime un container ainsi que les tâches qui s'y trouve
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void remContainer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (currentIndex > 2)
                        m_containers.RemoveAt(currentIndex);
                ComboBoxItem item = comboBox.SelectedItem as ComboBoxItem;
                if (req.checkContainer(bdd, item.Content.ToString()))
                {
                    if (item.Content.ToString() != "Boite de réception" && item.Content.ToString() != "Tâches de la journée" && item.Content.ToString() != "Tâches prioritaire")
                    {
                        MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Voulez vous vraiment supprimer le container ainsi que toutes les tâches et sous-tâches qui le composent? ", "Confirmation de suppression", System.Windows.MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (messageBoxResult == MessageBoxResult.Yes)
                        {
                            currentIndex = 0;
                            comboBox.SelectedIndex = 0;
                            container c = req.getSContainer(bdd, item.Content.ToString());
                            req.supContainer(bdd, c);
                            List<taches> remTask = req.getTachesContainer(bdd, c.label);
                            foreach (taches t in remTask)
                            {
                                req.supTaches(bdd, t);
                            }
                            comboBox.Items.Remove(item);
                            MessageBox.Show("Le container et les tâches et sous-tâches reliées ont bien été supprimées", "Media Task Manager", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                        MessageBox.Show("Impossible de supprimer ce container", "Media Task Manager", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
                else
                    MessageBox.Show("Vous n'avez pas enregistré l'ajout de ce container", "Media Task Manager", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            catch (Exception)
            {
                MessageBox.Show("Une erreur est survenu lors de la supression du container : " + "Veuillez réessayer ultérieurement et redémarrer l'application", "Media Task Manager", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /*                      Recherche une tâche                         */
        private void search_Click(object sender, RoutedEventArgs e)
        {
            CustomTreeViewItem selected = treeView.SelectedItem as CustomTreeViewItem;
            foreach (CustomTreeViewItem item in m_containers[currentIndex].m_Tasks)
            {
                if (searchText.Text == item.Title.Text)
                {
                    selected.IsSelected = false;
                    item.IsSelected = true;
                }
                foreach (CustomTreeViewItem subitem in item.Items)
                {
                    if (searchText.Text == subitem.Title.Text)
                    {
                        selected.IsSelected = false;
                        subitem.IsSelected = true;
                    }
                }
            }
        }
    }

    public class Container
    {
        public string Name;
        public ObservableCollection<CustomTreeViewItem> m_Tasks { get; set; }
        private mediametrieEntities bdd;
        private requete req;

        public Container()
        {
            bdd = new mediametrieEntities();
            req = new requete();
            this.m_Tasks = new ObservableCollection<CustomTreeViewItem>();
        }

        public void removeItem(CustomTreeViewItem item)
        {
            try
            {
                int i = 0;
                int index = -1;
                while (i < m_Tasks.Count && index < 0)
                {
                    index = m_Tasks[i].Items.IndexOf(item);
                    ++i;
                }

                if (item.parent != null && index >= 0)
                {
                    int parent_index = m_Tasks.IndexOf(item.parent);
                    m_Tasks[parent_index].Items.Remove(item);
                }
                else if (item.parent == null)
                {
                    m_Tasks.Remove(item);
                }
                taches t = req.getSTaches(bdd, item.Title.Text);
                req.supTaches(bdd, t);

            }
            catch (Exception)
            {
                MessageBox.Show("Une erreur est survenu lors de l'enregistrement des tâches en attente: " + "Veuillez réessayer ultérieurement et redémarrer l'application", "Media Task Manager", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void addItem(CustomTreeViewItem parent, string title, string beginDate, string endDate, string comment, bool done)
        {
            CustomTreeViewItem item = new CustomTreeViewItem(title);
            item.parent = parent;
            item.comment = comment;
            item.dateBegin = beginDate;
            item.dateEnd = endDate;
            item.chkBox.IsChecked = done;
            item.IsExpanded = true;
            if (parent != null)
            {
                item.chkBox.IsEnabled = !done;
                int index = (m_Tasks.IndexOf(parent) >= 0) ? m_Tasks.IndexOf(parent) : 0;
                m_Tasks[index].Items.Add(item);
            }
            else
            {
                item.chkBox.IsEnabled = true;
                m_Tasks.Add(item);
            }
        }
    }

    public class CustomTreeViewItem : TreeViewItem
    {
        public CustomTreeViewItem(string title)
        {
            bdd = new mediametrieEntities();
            req = new requete();
            Title.Text = title;
            chkBox.Content = Title;
            Title.AddHandler(TextBlock.MouseDownEvent, new MouseButtonEventHandler(change_label));
            chkBox.AddHandler(CheckBox.CheckedEvent, new RoutedEventHandler(chkBox_Checked));
            Header = chkBox;
        }

        public int Contains(string name)
        {
            for(int i = 0; i < this.Items.Count; ++i)
            {
                CustomTreeViewItem item = this.Items[i] as CustomTreeViewItem;
                if (item.Title.Text == name)
                    return 0;
            }
            return -1;
        }

        private void change_label(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                TextBox textBox = new TextBox();
                textBox.Text = Title.Text;

                chkBox.Content = textBox;
                textBox.KeyDown += (o, ev) =>
                {
                    if (ev.Key == Key.Enter || ev.Key == Key.Escape)
                    {
                        if (Title.Text != textBox.Text)
                        {

                            if (req.checkTaches(bdd, Title.Text) == true)
                            {
                                taches t = req.getSTaches(bdd, Title.Text);
                                req.supTaches(bdd, t);
                            }
                            if (req.checkTaches(bdd, textBox.Text) == true)
                            {
                                MessageBox.Show("Déjà une tache avec ce nom");
                                textBox.Text = Title.Text;
                                return;
                            }
                        }
                        Title.Text = textBox.Text;
                        chkBox.Content = Title;
                        ev.Handled = true;
                    }
                };
                textBox.LostFocus += (o, ev) =>
                {
                    Title.Text = textBox.Text;
                    chkBox.Content = Title;
                    ev.Handled = true;
                };
            }
            else
                this.IsSelected = true;
            e.Handled = true;
        }

        private void chkBox_Checked(object sender, RoutedEventArgs e)
        {
            this.IsSelected = true;
            foreach(CustomTreeViewItem item in this.Items)
            {
                item.chkBox.IsChecked = true;
            }
        }

        private mediametrieEntities bdd;
        private requete req;
        public CustomTreeViewItem parent;
        public CheckBox chkBox = new CheckBox();
        public TextBlock Title = new TextBlock();
        public string comment;
        public string dateBegin;
        public string dateEnd;
    }
}