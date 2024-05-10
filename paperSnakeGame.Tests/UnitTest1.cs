using paperSnakeGame;
using Moq;
using FluentAssertions;
using System.IO;

namespace paperSnakeGame.Tests
{
    [TestFixture]
    public class GameTests
    {
        [Test]
        public void NicknameTextBox_GotFocus_ShouldClearText_IfDefaultText()
        {
            // Arrange
            var window = new MainWindow();
            window.nicknameTextBox.Text = "Enter Nickname";

            // Act
            window.NicknameTextBox_GotFocus(window.nicknameTextBox, null);

            // Assert
            window.nicknameTextBox.Text.Should().BeEmpty("Текстовое поле должно быть очищено при получении фокуса.");
            Console.WriteLine("Тест NicknameTextBox_GotFocus выполнен успешно: поле очищено.");
        }

        [Test]
        public void SaveScore_ShouldSaveScore_IfHigherThanExisting()
        {
            // Arrange
            var window = new MainWindow();
            string testNickname = "testUser";
            int testScore = 25;

            // Act
            window.SaveScore(testNickname, testScore);

            // Assert
            // Проверка, что файл содержит новый счет
            var scores = File.ReadAllLines("Assets/scores.txt");
            scores.Should().Contain($"{testNickname}: {testScore}", "Новый счет должен быть сохранен в файле.");
            Console.WriteLine($"Тест SaveScore выполнен успешно: счет {testScore} сохранен для пользователя {testNickname}.");
        }

        [Test]
        public void LoadLeaderboard_ShouldLoadTop10Scores()
        {
            // Arrange
            var window = new MainWindow();

            // Act
            window.LoadLeaderboard();

            // Assert
            Assert.That(window.LeaderboardPanel.Children.Count, Is.LessThanOrEqualTo(10), "Таблица лидеров должна содержать не более 10 записей.");
            Console.WriteLine("Тест LoadLeaderboard выполнен успешно: загружено не более 10 результатов.");
        }
    }
}