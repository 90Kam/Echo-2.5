using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;  // DODANE dla Wait

internal class Program
{
    private static void Main(string[] args)
    {
        int num = 150;
        int millisecondsTimeout = 300;
        IWebDriver webDriver = null;
        while (webDriver == null)
        {
            Console.WriteLine("Na której przeglądarce chcesz pracować?              Google Chrome - wpisz '1'           Mozilla Firefox - wpisz '2'");
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
        var jsExecutor = (IJavaScriptExecutor)webDriver;  // Poprawione
        var wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(15));  // NOWE: Wait

        while (true)
        {
            Console.WriteLine("POCZĄTEK SESJI ODŚWIEŻANIA");
            webDriver.Navigate().Refresh();
            Thread.Sleep(9000);
            Console.WriteLine("Strona została odświeżona");

            // POPRAWIONA PĘTLA - zastępuje starą for
            int current = 1;
            while (current <= num)
            {
                Thread.Sleep(millisecondsTimeout);
                bool flag = false;
                while (!flag && current <= num)
                {
                    try
                    {
                        // SCROLL - ładuje virtual listę
                        jsExecutor.ExecuteScript("window.scrollBy(0, 600);");
                        Thread.Sleep(1500);

                        // NOWY RELATYWNY XPATH - szuka wszystkich przycisków resubmit/odśwież
                        string xpathToFind = "//a[contains(@class, 'resubmit') or contains(@data-ctx, 'resubmit') or contains(@href, 'resubmit')] | //button[contains(text(), 'Odśwież') or contains(text(), 'Refresh') or contains(@aria-label, 'odśwież')]";

                        var buttons = wait.Until(d => d.FindElements(By.XPath(xpathToFind)));

                        if (buttons.Count >= current)
                        {
                            var btn = buttons[current - 1];
                            wait.Until(d => btn.Displayed && btn.Enabled);
                            btn.Click();
                            flag = true;
                            Console.WriteLine($"Odświeżono fracht {current - 1} (znaleziono {buttons.Count} przycisków)");
                            current++;
                        }
                        else
                        {
                            Console.WriteLine($"Za mało przycisków ({buttons.Count} < {current}) - REFRESH");
                            webDriver.Navigate().Refresh();
                            Thread.Sleep(3000);
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        Console.WriteLine($"Brak przycisku {current} - REFRESH");
                        webDriver.Navigate().Refresh();
                        Thread.Sleep(2000);
                        flag = true;
                    }
                    catch (ElementClickInterceptedException)
                    {
                        try
                        {
                            Thread.Sleep(1000);
                            var fallback = wait.Until(d => d.FindElement(By.XPath("//button[contains(text(), 'Odśwież')] | //*[contains(text(), 'Refresh')] | /html/body/div[1]/div/div/div/div[10]/div/button")));
                            fallback.Click();
                            Console.WriteLine($"Fallback dla {current}");
                        }
                        catch { Console.WriteLine("Fallback fail"); }
                        flag = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Błąd {current}: {ex.Message}");
                        flag = true;
                    }
                }
            }

            Console.WriteLine("KONIEC SESJI ODŚWIEŻANIA");
            Thread.Sleep(900000);  // 15 min pauza
        }
    }
}
