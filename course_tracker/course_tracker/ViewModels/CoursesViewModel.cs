using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using course_tracker.Models;
using course_tracker.Views;
using Xamarin.Forms;

namespace course_tracker.ViewModels
{
    public class CoursesViewModel : BaseViewModel
    {
        public ObservableCollection<Course> Courses { get; set; }
        public Command LoadTermCommand { get; set; }

        Term _term;
        public Term Term
        {
            get { return _term; }
            set { SetProperty(ref _term, value); }
        }

        bool canAddCourse = true;
        public bool CanAddCourse
        {
            get { return canAddCourse; }
            set { SetProperty(ref canAddCourse, value); }
        }

        public CoursesViewModel(Term term)
        {
            Term = term;
            Title = $"{term.Title}'s courses";

            Courses = new ObservableCollection<Course>();
            LoadTermCommand = new Command(async () => await LoadTerm());

            MessagingCenter.Subscribe<NewCoursePage, Course>(this, "AddCourse", async (obj, course) =>
            {
                await LoadTerm();
            });

            MessagingCenter.Subscribe<NewTermPage, Term>(this, "AddTerm", async (obj, t) =>
            {
                await LoadTerm();
            });
        }

        private static object coursesLock = new object();
        async Task LoadTerm()
        {
            IsBusy = true;
            try
            {
                Term = await SqliteConn.Table<Term>().FirstOrDefaultAsync(t => t.Id == Term.Id);
                Title = $"{Term.Title}'s courses";
                var courses = await SqliteConn.Table<Course>().Where(c => c.TermId == Term.Id).OrderBy(term => term.Start).ToListAsync();

                // Loading the data causes the refresh to trigger
                lock (coursesLock)
                {
                    CanAddCourse = courses.Count < 6;
                    Courses.Clear();
                    foreach (var course in courses)
                    {
                        Courses.Add(course);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task DeleteCourses(Course course)
        {
            await SqliteConn.DeleteAsync(course);
            await LoadTerm();
        }
    }
}