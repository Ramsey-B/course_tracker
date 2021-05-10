using System;
using System.Diagnostics;
using System.Threading.Tasks;
using course_tracker.Models;
using course_tracker.Views;
using Xamarin.Forms;

namespace course_tracker.ViewModels
{
    public class CourseDetailsViewModel : BaseViewModel
    {
        private Course course;
        public Course Course
        {
            get { return course; }
            set { SetProperty(ref course, value); }
        }

        public Command LoadCourseCommand { get; set; }

        private Assessment objectiveAssessment;
        public Assessment ObjectiveAssessment
        {
            get { return objectiveAssessment; }
            set { SetProperty(ref objectiveAssessment, value); }
        }

        private bool hasObjectiveAssessment = false;
        public bool HasObjectiveAssessment
        {
            get { return hasObjectiveAssessment; }
            set { SetProperty(ref hasObjectiveAssessment, value); }
        }

        private Assessment performanceAssessment;
        public Assessment PerformanceAssessment
        {
            get { return performanceAssessment; }
            set { SetProperty(ref performanceAssessment, value); }
        }

        private bool hasPerformanceAssessment = false;
        public bool HasPerformanceAssessment
        {
            get { return hasPerformanceAssessment; }
            set { SetProperty(ref hasPerformanceAssessment, value); }
        }

        private string notes = string.Empty;
        public string Notes
        {
            get { return notes; }
            set { SetProperty(ref notes, value); }
        }

        public CourseDetailsViewModel(Course course)
        {
            Course = course;
            Title = course.Title;
            Notes = course.Notes;
            LoadCourseCommand = new Command(async () => await LoadCourse());

            MessagingCenter.Subscribe<NewAssessmentPage, Assessment>(this, "AddObjectiveAssessment", async (obj, assessment) =>
            {
                await LoadCourse();
            });
            MessagingCenter.Subscribe<NewAssessmentPage, Assessment>(this, "AddPerformanceAssessment", async (obj, assessment) =>
            {
                await LoadCourse();
            });
            MessagingCenter.Subscribe<NewCoursePage, Course>(this, "AddCourse", async (obj, newCourse) =>
            {
                await LoadCourse();
            });
            MessagingCenter.Subscribe<NewNotesPage, Course>(this, "AddNotes", async (obj, newCourse) =>
            {
                await LoadCourse();
            });
        }

        private static object assessmentsLock = new object();
        private async Task LoadCourse()
        {
            IsBusy = true;
            HasObjectiveAssessment = false;
            HasPerformanceAssessment = false;
            try
            {
                Course = await SqliteConn.Table<Course>().FirstOrDefaultAsync(c => c.Id == Course.Id);
                var assessments = await SqliteConn.Table<Assessment>().Where(a => a.CourseId == Course.Id).ToListAsync();

                lock (assessmentsLock)
                {
                    foreach (var assessment in assessments)
                    {
                        if (assessment.Type == AssessmentType.Objective)
                        {
                            ObjectiveAssessment = assessment;
                            HasObjectiveAssessment = true;
                        }
                        else
                        {
                            PerformanceAssessment = assessment;
                            HasPerformanceAssessment = true;
                        }
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

        public async Task DeleteAssessment(Assessment assessment)
        {
            await SqliteConn.DeleteAsync(assessment);
            await LoadCourse();
        }

        public async Task UpdateCourseNotifications()
        {
            await SqliteConn.UpdateAsync(Course);
        }

        public async Task UpdateAssessmentNotifications(Assessment assessment)
        {
            await SqliteConn.UpdateAsync(assessment);
        }
    }
}