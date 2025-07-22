Clickless Mouse ułatwia użytkowanie komputera osobą z RSI (repetitive strain injury), zespołem cieśni nadgarstka, niektórymi niepełnosprawnościami ruchowymi oraz innymi problemami zdrowotnymi.

Ta aplikacja umożliwia używanie myszki bez klikania - jedynie poprzez jej przesuwanie. Poprzez reagowanie na poruszanie myszą przez użytkownika program symuluje lewy/prawy przycisk myszy, podwójne kliknięcie lewym przyciskiem myszy oraz przytrzymywanie lewego/prawego przycisku myszy.

Clickless Mouse można używać z klawiaturą wirtualną do pisania poprzez poruszanie myszką (np. [Free Virtual Keyboard](https://freevirtualkeyboard.com)). Używając Clickless Mouse wraz z wirtualną klawiaturą użytkownik może w pełni kontrolować komputer poprzez poruszanie myszką.

Kiedy chcesz kliknąć/przytrzymać przycisk myszy: zatrzymaj myszkę, poczekaj aż pojawią się kwadraty, a następnie przesuń kursor do wybranego kwadratu:
* Górny środkowy kwadrat = podwójne kliknięcie lewym przyciskiem myszy
* dGórny lewy kwadrat = kliknięcie lewym przyciskiem myszy
* *Górny prawy kwadrat = kliknięcie prawym przyciskiem myszy
* Lewy kwadrat = włącz/wyłącz przytrzymanie lewego przycisku myszy
* Prawy kwadrat = włącz/wyłącz przytrzymanie prawego przycisku myszy

![ResourceImage](Assets/1pl.jpg)
 
Kiedy kursor myszy znajduje się wystarczająco długo w kwadracie (w zależności od ustawienia czasu kursora w kwadracie do rejestracji kliknięcia), przesunie się automatycznie do poprzedniej pozycji, aby zasymulować akcję na podstawie ostatnio odwiedzonego kwadratu.

Jeśli kursor myszy jest zbyt blisko górnej krawędzi ekranu, kwadraty dla kliknięcia LPM, kliknięcia PPM i dwukliknięcia LPM są pokazywane poniżej kursora myszy.

Gdy kursor myszy jest zbyt blisko lewej lub prawej krawędzi ekranu rozmiar wszystkich kwadratów zostaje zmniejszony tak aby przynajmniej 25% kwadratu było widoczne. Maksymalne domyślne zmniejszenie rozmiaru kwadratu to 60% oryginalnej wielkości.
Wielkość kwadratów nie zmniejsza się, jeśli kursor jest na tyle blisko krawędzi ekranu, że najmniejszy rozmiar nie wystarczy do pokazania kwadratu.

Istnieje możliwość wyłączenia niepotrzebnych kwadratów.

Przesuwanie ekranu - gdy tryb ten jest włączony, przesunięcie kursora do krawędzi ekranu powoduje naciśnięcie klawiszy: góra dla górnej krawędzi, dół dla dolnej krawędzi, lewo dla lewej krawędzi i prawo dla prawej krawędzi. Gdy przesuwanie ekranu jest aktywne, kwadraty nie są pokazywane kiedy kursor myszy znajduje się na krawędzi ekranu.

Clickless Mouse działa tylko w programach i grach, które zostały uruchomione w trybie bezramkowym lub okienkowym (tryb pełnoekranowy nie jest wspierany).

**Pierwsze kroki:**

1. Podaj przekątną ekranu, a następnie naciśnij przycisk 'Ustaw zalecaną wielkość kwadratu'.
2. Zdecyduj z jakich funkcji myszki chcesz skorzystać. Większość użytkowników wybiera 'Klik LPM', 'Klik PPM', 'Dwuklik LPM' oraz 'Przytrzymywanie LPM'.
3. Jeśli posiadasz niepełnosprawność ruchową rozważ zwiększenie czasu bezczynności kursora zanim pojawią się kwadraty, czasu na rozpoczęcie ruchu myszą po pojawieniu się kwadratów oraz rozmiaru kwadratów.

**Najniższe możliwe wartości (program ignoruje niższe wartości i używa poniższe zamiast nich):**

* Czas bezczynności kursora zanim pojawią się kwadraty [ms]: 100
* Czas na rozpoczęcie ruchu myszą po pojawieniu się kwadratów [ms]: 300
* Czas kursora w kwadracie do zarejestrowania kliknięcia [ms]: 10
* Rozmiar [px]: 10
* Szerokość krawędzi [px]: 1
* Minimalny rozmiar [%]: 10