using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

internal class Program
{
    private static void Main(string[] args)
    {
        int num = 150;
        int millisecondsTimeout = 300;
        IWebDriver webDriver = null;
        while (webDriver == null)
        {
            Console.WriteLine("Na której przeglądarce chcesz pracować?\r\n                Google Chrome - wpisz '1'\r\n                Mozilla Firefox - wpisz '2'");
            string text = Console.ReadLine();
            Console.WriteLine("Podaj liczbę frachtów do odświeżenia: ");
            num = int.Parse(Console.ReadLine());
            Console.WriteLine("Podaj czas co jaki ma się odświeżać każdy fracht (ms): ");
            millisecondsTimeout = int.Parse(Console.ReadLine());
            if (text == "1")
            {
                webDriver = new ChromeDriver();
                Console.WriteLine("Wybrana przeglądarka: Google Chrome");
            }
            else if (text == "2")
            {
                webDriver = new FirefoxDriver();
                Console.WriteLine("Wybrana przeglądarka: Mozilla Firefox");
            }
            else
            {
                Console.WriteLine("Nie wybrano przeglądarki. Spróbuj ponownie");
            }
        }
        webDriver.Navigate().GoToUrl("https://auth.platform.trans.eu/accounts/login?login_challenge=6718dfa6b2be49409600c3acb63342a2");
        webDriver.Manage().Window.Maximize();
        Thread.Sleep(10000);
        webDriver.FindElement(By.Name("login")).SendKeys("1172359-14");
        IWebElement webElement = webDriver.FindElement(By.Name("password"));
        Thread.Sleep(5000);
        webElement.SendKeys("Romanbit111!");
        IWebElement webElement2 = webDriver.FindElement(By.XPath("//button[text()='Zaloguj']"));
        Thread.Sleep(5000);
        if (webElement2.Enabled)
        {
            webElement2.Click();
            Console.WriteLine("aktywny");
        }
        else
        {
            IWebElement frameElement = webDriver.FindElement(By.CssSelector("iframe[src*='recaptcha']"));
            webDriver.SwitchTo().Frame(frameElement);
            webDriver.FindElement(By.CssSelector(".recaptcha-checkbox")).Click();
            Console.WriteLine("nieaktywny");
            Thread.Sleep(500);
            webDriver.SwitchTo().DefaultContent();
            Thread.Sleep(5);
            webElement2.Click();
        }
        Thread.Sleep(15000);
        webDriver.FindElement(By.LinkText("Frachty")).Click();
        Thread.Sleep(5000);
        _ = (IJavaScriptExecutor)webDriver;
        while (true)
        {
            Console.WriteLine("POCZĄTEK SESJI ODŚWIEŻANIA");
            webDriver.Navigate().Refresh();
            Thread.Sleep(9000);
            Console.WriteLine("Strona została odświeżona");
            for (int i = 1; i <= num; i++)
            {
                Thread.Sleep(millisecondsTimeout);
                bool flag = false;
                while (!flag)
                {
                    try
                    {
                        Thread.Sleep(millisecondsTimeout);
                        ///html/body/div[1]/div/div/div/div[1]/article/div[2]/div/div/div[2]/div/table[2]/tbody/tr[1]/td[9]/div/div/div[3]/a/svg/path
                        string xpathToFind = $"/html/body/div[1]/div/div/div/div[1]/article/div[2]/div/div/div[2]/div/table[2]/tbody/tr[{i}]/td[9]/div/div/div[3]/a";
                        //string xpathToFind = $"//*[@id='app']/div/div/div/div[1]/div[1]/article/div[2]/div/div/div[2]/div/table[2]/tbody/tr[{i}]/td[9]/div/div/div[3]";
                        //string xpathToFind = $"/html/body/div[1]/div/div/div/div[1]/article/div[2]/div/div/div[2]/div/table[2]/tbody/tr[{i}]/td[9]/div/div/div[3]/a/svg";
                        //string xpathToFind = $"/html/body/div[1]/div/div/div/div[1]/article/div[2]/div/div/div[2]/div/table[2]/tbody/tr[{i}]/td[9]/div/div/div[3]/div/a";
                        IWebElement webElement3 = webDriver.FindElement(By.XPath(xpathToFind));
                        Thread.Sleep(millisecondsTimeout);
                        //Thread.Sleep(1000);
                        webElement3.Click();
                        flag = true;
                        Console.WriteLine("Odświeżono fracht nr: " + (i - 1));
                    }
                    catch (NoSuchElementException)
                    {
                        Console.WriteLine($"Nie znaleziono przycisku dla XPath z indeksu: {i}");
                        flag = true;
                    }
                    catch (ElementClickInterceptedException)
                    {
                        try
                        {
                            Thread.Sleep(1000);
                            
                            //IWebElement webElement4 = webDriver.FindElement(By.CssSelector("div#app>div>div>div>div:nth-of-type(9)>div>div>div>div>button>span"));
                            IWebElement webElement4 = webDriver.FindElement(By.XPath("/html/body/div[1]/div/div/div/div[10]/div[1]/div/div/div[1]/button"));
                            Thread.Sleep(1000);
                            webElement4.Click();
                            Console.WriteLine($"Przycisk dla XPath z indeksu: {i} jest blokowany przez inny element.");
                            Thread.Sleep(1000);
                        }
                        catch (NoSuchElementException)
                        {
                            Console.WriteLine($"Nie znaleziono elementu blokującego dla XPath z indeksu: {i}");
                            flag = true;
                        }
                    }
                    catch (Exception ex4)
                    {
                        Console.WriteLine("Wystąpił błąd: " + ex4.Message);
                        flag = true;
                    }
                }
            }
            Console.WriteLine("KONIEC SESJI ODŚWIEŻANIA");
            Thread.Sleep(500);
            Thread.Sleep(2000);
            Thread.Sleep(900000);
        }
    }
}
