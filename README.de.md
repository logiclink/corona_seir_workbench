# Corona SEIR Workbench [![English version](images/en.svg)](README.md)
Die [SARS-COV-2](https://de.wikipedia.org/wiki/SARS-CoV-2) Pandemie beeinflusst bereits seit Monaten unser Leben. Die Wirksamkeit von Maßnahmen gegen die Pandemie kann anhand epidemiologischer Modelle überprüft und vorausgesagt werden. Die Corona SEIR Workbench verwendet hierfür ein SEIR Model und kombiniert eine graphische Ausgabe der Ergebnisse mit einer einfachen Parametereingabe. Die modellierten Daten können länderweise mit den SARS-COV-2 Infektionsdaten der Johns-Hopkins-Universität verglichen werden. Für Deutschland können zusätzlich die R₀-Werte des Robert-Koch-Instituts angezeigt werden.

## Aktueller Stand der COVID-19-Pandemie
Seit Dezember 2019 hält der Corona Virus SARS-COV-2 und seine [COVID-19](https://de.wikipedia.org/wiki/COVID-19-Pandemie) Erkrankung die Welt mit einer [Pandemie](https://de.wikipedia.org/wiki/Pandemie) in Atem. Dabei war das Auftreten einer vergleichbaren Pandemie nur eine Frage der Zeit. Schätzungen gehen von einem Auftreten vergleichbarer Pandemien alle 20-30 Jahre aus.<sup>[1](#f1)</sup>

Da Stand heute noch kein Impfstoff flächendeckend zur Verfügung steht, werden viele verschiedene, nicht-medizinische Maßnahmen zur Eindämmung der Pandemie eingesetzt. Diese reichen von Alltagsmasken über Abstände zwischen Personen, Schließung von öffentlichen Einrichtungen, Sportstätten und der Gastronomie bis zu Kontaktbeschränkungen alle mit dem Ziel, die Virus-Ausbreitung zu reduzieren. Teststrategien und [Kontaktnachverfolgung](https://www.logiclink.de/2020/10/22/superspreading-covid-19-pandemie/) sollen infizierte Personen möglichst schnell erkennen und dann in einer Quarantäne isolieren. Alle diese Maßnahmen sollen das Infektionsgeschehen kontrollieren und trotzdem möglichst wenig Einschränkungen für die Bevölkerung und wenig negative wirtschaftliche Folgen haben.

Voraussetzung für die Kontrolle der Pandemie ist eine möglichst gute Datenbasis. Diese wird in Deutschland vom [Robert Koch-Institut](https://www.rki.de/) über die Gesundheitsämter in ermittelt. International trägt das [Center for Systems Science and Engineering (CSSE)](https://systems.jhu.edu/) der Johns-Hopkins-Universität [SARS-COV-2 Infektionszahlen](https://www.arcgis.com/apps/opsdashboard/index.html#/bda7594740fd40299423467b48e9ecf6) zusammen und stellt diese zur Verfügung.

Das zentrale Ziel der Pandemie-Kontrolle ist es, die Überlastung des Gesundheitssystems zu vermeiden, damit alle an COVID 19 Erkrankten eine möglichst optimale medizinische Versorgung bekommen. Darüber hinaus ist eine möglichst geringe Infektionsrate wünschenswert, um die gesundheitlichen Folgen auf die Bevölkerung zu minimieren. 

Auf der Basis der ermittelten Daten können [Epidemie-Modelle](https://en.wikipedia.org/wiki/Compartmental_models_in_epidemiology) Voraussagen über das Infektionsgeschehen für die Zukunft treffen. Infektionszahlen entwickeln sich exponentiell. Daher können die Zuwachszahlen an Erkrankten schnell aus dem Ruder laufen und das Gesundheitssystem überlasten. Aus diesem Grund sind ein frühes Einschreiten und eine Überprüfung der getroffenen Maßnahmen essentiell für eine Kontrolle der Pandemie.

## Das SEIR Modell
Das [SEIR-Modell](https://de.wikipedia.org/wiki/SEIR-Modell) gehört zur Klasse der Kompartiment-Modelle, die Personen in Kompartimente einsortiert. Das SEIR-Modell teilt die Bevölkerung in die vier Kompartimente „**S**usceptible“ (anfällige Personen), „**E**xposed“ (exponierte Personen), „**I**nfectious“ (infektiöse Personen) und „**R**ecovered“ (erholte bzw. gestorbene Personen) ein:

<p align="center"><img src="images/SEIR.svg" alt="Kompartimente des SEIR-Modells"></p>
<p align="center"><sup>Kompartimente des SEIR-Modells</sup></p>

Bei der Modellierung wandern Personen von einem Kompartiment ins nächste. Die Berechnung findet je Zeiteinheit statt. Als Zeiteinheit wird meist ein Tag verwendet.

Am Anfang einer Epidemie sind so gut wie alle Personen gesund und im Kompartiment „**S**usceptible“. Alle Personen in diesem Kompartiment können mit dem Corona-Virus infiziert werden. Nach einer Infektion wandert eine Person in das Kompartiment „**E**xposed“ und verbleibt dort für die mittlere Inkubationsdauer der Erkrankung. Zu dieser Zeit ist die Person noch nicht für andere Personen ansteckend. Eine Studie<sup>[2](#f2)</sup> im Januar hat die mittlere Inkubationszeit mit 5,2 Tagen angegeben. Eine ausführliche Diskussion ist bei Byrne et al.<sup>[3](#f3)</sup> zu finden.

Sobald die kranke Person Viren verteilt und andere Personen infizieren kann, kommt Sie aus dem Kompartiment „**E**xposed“ in das Kompartiment „**I**nfectious“. In diesem „Topf“ befinden sich die Personen, die die Pandemie antreiben. Die kranke Person bleibt in diesem Kompartiment für die mittlere Infektionszeit von 2,9 Tagen<sup>[4](#f4)</sup>. 

Die mittlere Infektionszeit ist nicht mit der maximalen Infektionsdauer zu verwechseln, die bei der Quarantäne einer kranken Person eine Rolle spielt und bis zu 14 Tage lang sein kann. Die mittlere Infektionsdauer wird auch durch die Diagnostik beeinflusst, da eine kranke Person häufig bei Symptombeginn getestet und dann isoliert wird.

Nachdem eine Person die Krankheit überstanden hat und nicht mehr infektiös ist, wandert diese aus dem Kompartiment „**I**nfectious“ in das Kompartiment „**R**ecovered“.  Sie ist zu diesem Zeitpunkt immun gegen eine erneute Ansteckung mit SARS-COV-2. Neben der Erholung kann die Person ebenso an COVID 19 versterben. Das SEIR Modell trifft hier keine Unterscheidung und sortiert die Person in beiden Fällen in das Kompartiment „**R**ecovered“.

Seit Dezember 2020 laufen immer mehr Impfprogramme gegen SARS-COV-2 an. In Europa und den USA werden die mRNA Impfstoffe von Biontech/Pfizer und Moderna mit einer hohen Wirksamkeit von 94% - 95% derzeit eingesetzt.

Es gibt verschiedene [Berechnungsgrundlagen](https://de.wikipedia.org/wiki/Mathematische_Modellierung_der_Epidemiologie), um geimpfte Personen innerhalb eines SEIR-Modells zu berücksichtigen. So kann das SEIR-Modell um ein zusätzliches Kompartiment „**V**accinated“ für geimpfte Personen zu einem SEIRV-Modell erweitert werden.<sup>[5](#f5)</sup> Nach einer Impfung wandern Personen aus dem „**S**usceptible“ Kompartiment in das „**V**accinated“ Kompartiment und können dort nicht mehr infiziert werden.

Diese Sichtweise ist stark vereinfacht, da zwei Impfungen in einem zeitlichen Abstand von 3 bzw. 4 Wochen stattfinden und der Impfschutz sich graduell über mehrere Wochen Wirksamkeit aufbaut. Ein teilweiser Impfschutz besteht frühestens 12 Tage<sup>[6](#f6)</sup> nach der ersten Impfung. Daher können geimpfte Personen nicht komplett aus der Menge der anfälligen Personen herausgenommen werden.

Um die zu berücksichtigen kann das „**V**accinated“ Kompartiment als Teilmenge der anfälligen Personen im Kompartiment „**S**usceptible“ betrachtet werden:

<p align="center"><img src="images/SV.svg" alt="Geimpft-Kompartiment"></p>
<p align="center"><sup>Geimpft-Kompartiment</sup></p>

Mit diesem Modell können anfällige Personen nach einer Impfung zu einem von der Wirksamkeit abhängigen Prozentsatz immer noch infiziert werden. Das SEIRV-Modell besteht damit ausfolgenden Kompartimenten:

<p align="center"><img src="images/SEIRV.svg" alt="Kompartimente des SEIRV-Modells"></p>
<p align="center"><sup>Kompartimente des SEIRV-Modells</sup></p>

## Die Mathematik hinter dem SEIR Modell
Während der Modellierung des SEIR-Modells wird die Veränderung aller Kompartimente je Zeiteinheit berechnet. Zusätzlich wird im SEIRV-Modell vor der Berechnung der Veränderung der SEIR-Kompartimente die Veränderung zwischen dem Teil-Kompartiment *V* und dem Kompartiment *S* bestimmt.

Im ersten Schritt werden daher innerhalb des Kompartiments *S* alle geimpften Personen in das Teilkompartiment *V* verschoben. Es wird eine konstante Zahl an Impfungen pro Tag angenommen. Aus diesem Grund wird die Bevölkerungszahl *N* als Bezugsgröße verwendet und mit einem konstanten Faktor multipliziert:

<p align="center"><img src="images/SEIRV%20delta%20Vaccinated.svg" alt="SEIRV ΔVaccinated"></p>

Nachdem die geimpften Personen ΔS<sub>v</sub> bestimmt wurden, können diese im nächsten Schritt bei der Berechnung der ΔS<sub>i</sub> anfälligen Personen berücksichtigt werden. In diesem Schritt werden Personen im Kompartiment *S* durch infektiöse Personen im Kompartiment *I* infiziert. Hierfür ist neben der Menge der infektiösen Personen die [Basisreproduktionszahl R₀](https://de.wikipedia.org/wiki/Basisreproduktionszahl) ausschlaggebend:

<p align="center"><img src="images/SEIRV%20delta%20Susceptible.svg" alt="SEIRV ΔSusceptible"></p>

Für SARS-COV-2 wurde ein R₀-Wert von 3,4<sup>[7](#7)</sup> für eine ungebremste Verbreitung ermittelt. Das heißt im Mittel steckt ein Infizierter 3,4 weitere anfällige Personen an. Ein R₀-Wert größer 1 sorgt für ein exponentielles Wachstum von Infektionen, während ein R₀-Wert kleiner 1 zu einer abnehmende Zahl von neu infizierten Personen sorgt.

Von der Gesamtzahl der anfälligen Personen, werden die geimpften Personen mit einem Faktor für die Impfwirksamkeit abgezogen, da diese Personen nicht infiziert werden können. In einem SEIR Modell ohne Berücksichtigung von Impfungen würden hier alle Personen aus dem Kompartiment S verwendet.

Die Änderung der infektiösen Personen ΔE ergibt sich aus der Inkubationsdauer:

<p align="center"><img src="images/SEIR%20delta%20Exposed.svg" alt="SEIR ΔExposed"></p>

Daher wächst die Zahl der infektiösen Personen mit einem gewissen Zeitverzug nach ihrer Infektion.

Nachdem die Krankheit überstanden ist oder die infektiöse Person isoliert wurde, wandert diese in das Kompartiment *R*. Die Anzahl ΔI ergibt sich aus der mittleren Infektiositätsdauer:

<p align="center"><img src="images/SEIR%20delta%20Infectious.svg" alt="SEIR ΔInfectious"></p>

Die verwendeten Symbole in den Gleichungen stehen für folgende Werte:

<p align="center"><img src="images/SEIRV%20Legende.svg" alt="SEIR Legende"></p>

Die neuen Personenanzahlen *S‘*, *E‘*, *I‘*, *R‘* und *V‘* in den einzelnen Kompartimenten *S*, *E*, *I*, *R* und *V* nach einer Zeiteinheit kann nach folgenden Formeln berechnet werden:

![SEIR next Susceptible](images/SEIRV%20next%20Vaccinated.svg)

![SEIR next Susceptible](images/SEIRV%20next%20Susceptible.svg)

![SEIR next Exposed](images/SEIRV%20next%20Exposed.svg)

![SEIR next Infectious](images/SEIR%20next%20Infectious.svg)

![SEIR next Recovered](images/SEIR%20next%20Recovered.svg)

## Die Workbench
Das Ziel der Corona Workbench war es eine Infrastruktur für die Modellierung einer Epidemie zu schaffen. Dazu gehört neben der Modellierung auch die graphische Darstellung der Ergebnisse je Zeiteinheit und die leichte Änderung von Modellierungsparametern. Um die Modellierung mit historischen Daten zu vergleichen sollten Bevölkerungsgrößen, Infektionszahlen und R₀-Werte parallel angezeigt werden können. Darüber hinaus sollte die Software leicht erweiterbar sein, um auch andere Modelle schnell zu realisieren.

### Architektur der Workbench
Wir haben uns bei der Corona SEIR Workbench für einen klassenbasierten Ansatz mit C# entscheiden, da sich die einzelnen Komponenten so deutlich besser isolieren und warten lassen. Darüber hinaus bietet .Net eine sehr gute Performance und C# verfügt über alle Möglichkeiten einer Hochsprache mit objektorientierten und funktionalen Sprachelementen. Mittels Linq können hier einfach Mengenoperationen eingebaut werden.

Für eine graphische Anzeige verwenden wir die [Microsoft Windows Forms Charting](https://docs.microsoft.com/de-de/dotnet/api/system.windows.forms.datavisualization.charting)<sup>[8](#8)</sup> Bibliothek, die mit Visual Studio kommt. Für die Benutzeroberfläche verwenden wir [XAML](https://de.wikipedia.org/wiki/Extensible_Application_Markup_Language) aus der [Windows Presentation Foundation](https://de.wikipedia.org/wiki/Windows_Presentation_Foundation) und kombinieren beides in einem .Net Core 3.1 Projekt.

Um die Daten möglichst vieler Länder darzustellen, benutzen wir die Infektionszahlen des [CSSE](https://systems.jhu.edu/) der Johns-Hopkins-Universität, die von [Datopian](https://datahub.io/core/covid-19) täglich nach Ländern aggregiert werden. Für historische R₀-Werte verwenden wir die vom Robert Koch-Institut täglich veröffentlichten [Nowcasting-Werte](https://www.rki.de/DE/Content/InfAZ/N/Neuartiges_Coronavirus/Projekte_RKI/Nowcasting_Zahlen.xlsx). Bevölkerungszahlen ermitteln wir aus dem API der [World Bank](https://www.worldbank.org/). Tägliche Impfzahlen werden über [Our World in Data](https://ourworldindata.org/covid-vaccinations) der Oxford Martin School und University of Oxford ermittelt. Alle Daten werden nur einmal täglich aus dem Web geladen und dann temporär für weitere Modellierungen gespeichert.

Einen auf Python basierten Ansatz der SEIR Modellierung von Corona hat Pina Merkert<sup>[9](#9)</sup> hat im Heise c‘t Magazin vorgestellt. Der Python Code ist ebenfalls auf [GitHub](https://github.com/pinae/SEIR-fit) zu finden ist.

### Die Benutzeroberfläche der Workbench
Die Benutzeroberfläche sollte möglichst einfach sein und mit Visual Studio leicht von C# Programmierern erweitert werden können. Das Hauptfenster gliedert sich in drei Blöcke:

![Benutzeroberfläche der Corona SEIR Workbench](images/Corona%20SEIR%20Workbench.png)
<p align="center"><sup>Benutzeroberfläche der Corona SEIR Workbench</sup></p>

Im oberen Bereich wird ein Diagramm angezeigt. Alle Werte werden je Datum auf der horizontalen Achse dargestellt. Die linke, vertikale Achse bezieht sich auf Fallzahlen während die rechte Achse R₀-Werte anzeigt. Tägliche Inzidenzwerte werden als Balken in der Graphik und Kästchen in der Legende ausgegeben. Zusätzlich wird für modellierte Gesamt-Fallzahlen, Tages-Fallzahlen und mittlere 7-Tages Fallzahlen eine Raute für den Wert angezeigt, an dem sich die Zahlen des aktuellen Tages verdoppelt haben. Im Tooltip wird der Wert unter dem Mauszeiger für das dargestellte Datum angezeigt.

Unterhalb des Diagramms können einzelne Kurven in dem Bereich „*Series*“ aus- und eingeblendet werden. Die Skalierung der Achsen erfolgt automatisch, so dass es meist Sinn macht, die Kurve der anfälligen Personen („**S**usceptible“) mit ihrem großen Werten auszublenden.

Die nächsten Parameter bestimmen das Land und den Modellierungszeitraum. Darunter können SEIR-Parameter für die mittlere Inkubationszeit, den mittleren Infektionszeitraum und die Basisreproduktionszahl R₀ eingestellt werden. Änderungen werden sofort übernommen, die Modellierung neu berechnet und die Ergebnisse im Diagramm dargestellt.

Neben einem konstanten R₀-Wert kann dieser auf der Basis der historischen Fallzahlen berechnet werden. Die Grundlage für die Berechnung des R₀-Wertes liefert eine „Solver“ Komponente, die das SEIR nach der Levenberg-Marquardt-Methode<sup>[10](#f10)</sup> löst. Im ersten Lösungsverfahren wird für jeden einzelnen Tag im SEIR Modell in einem zukünftigen Fenster von n-Tagen der R₀-Wert mit dem geringsten Fehler in den Fallzahlen berechnet. Je größer das Fenster ist, desto stärker wird die Kurve geglättet. Beim zweiten Verfahren wird der R₀-Wert mit der geringsten Abweichung in den Fallzahlen für ein bestimmtes Intervall berechnet. Beide „Solver“-Komponenten verwenden immer den größten R₀-Werte, der den kleinsten Fehler liefert, wenn die Fehler mehrerer R₀-Werte identisch sind.

Zum Schluss wird er Mittelwert des R₀-Wertes aus den letzten 5 Tagen ermittelt und dann für alle zukünftigen Zeiteinheiten verwendet. Dieser zukünftige Mittelwert kann manuell angepasst werden, um Maßnahmen des Infektionsschutzes im Modell zu berücksichtigen.

Unterhalb der Einstellungen für die Basisreproduktionszahl können Angaben zu Impfungen gemacht werden. Auf der Basis der tatsächlichen Impfungen wird der Tagesmittelwert und der Start der Impfkampagne berechnet. Sollten in dem Land noch keine Impfungen durchgeführt worden sein, oder keine Daten dazu existieren, so werden hier Standardwerte angezeigt. Die Anzahl der geimpften Personen pro Tag kann über einen Schieberegler geändert werden.

Zusätzlich wird die Impfwirksamkeit angegeben. Diese gibt an, wie hoch der Anteil geimpfter Personen ist, der nicht mehr infiziert werden kann. Auf der Basis der Zulassungsdaten von Biontech und Moderna wird hier standardmäßig 95% angezeigt. Dieser Wert kann ebenfalls über einen Schieberegler angepasst werden. Als letztes kann der Start der Impfkampagne manuell geändert werden.

Im unteren Teil des Hauptfensters werden alle Funktionen der Workbench als Schaltflächen angezeigt. „*Show Data*“ zeigt alle im Diagramm dargestellten Werte als Tabelle an. Von hier können die Werte über die Zwischenablage in andere Programme, wie z.B. Mcirosoft Excel übernommen werden. Mittels „*Export Chart*“ kann das Diagramm als Graphik in verschiedenen Formaten oder auch als CSV-Datei exportiert werden. 

Einstellungen des Diagramms und des SEIR Modells können mit der Schaltfläche „*Save Settings*“ gespeichert werden. Die gespeicherten Einstellungen werden automatisch beim Start der Anwendung geladen. Mit „*Reset Settings*“ werden diese auf die Standardeinstellungen zurückgesetzt. Die Schaltfläche „*Clear Settings*“ setzt wie „*Reset Settings*“ alle Einstellungen zurück, blendet jedoch alle Kurven im Diagramm aus. Diese Funktion ist für die Erstellung eigener Diagramme bequemer, da sonst viele Kurven erst ausgeblendet werden müssten.

### Ergebnisse der Workbench
Die einfachste Modellierung verwendet ein SEIR-Modell mit einer ungebremsten Ausbreitung des SARS-COV-2 Virus und einer konstanten Basisreproduktionszahl von 3,4 für Deutschland.

<p align="center"><img src="images/SEIR%20Germany%20without%20measures.svg" alt="SEIR-Modell für Deutschland"></p>
<p align="center"><sup>SEIR-Modell für Deutschland</sup></p>

In diesem Fall hätte sich fast die gesamte Bevölkerung im April und Mai diesen Jahres infiziert. Die tatsächlichen Fallzahlen der Johns-Hopkins-Universität für Deutschland zeigen jedoch einen anderen Verlauf:

<p align="center"><img src="images/SEIR%20Germany%20JHE.svg" alt="Fallzahlen der Johns-Hopkins-Universität vom 13.11.2020"></p>
<p align="center"><sup>Fallzahlen der Johns-Hopkins-Universität vom 13.11.2020</sup></p>

Der Grund für den unterschiedlichen Verlauf erklärt sich aus den R₀-Werten des Robert Koch-Instituts, die im April von 3,4 auf unter 1 gefallen sind:

<p align="center"><img src="images/SEIR%20Germany%20JHE%20+%20RKI%20R0.svg" alt="Fallzahlen der Johns-Hopkins-Universität vom 13.11.2020 mit R₀-Werten des Robert Koch-Instituts"></p>
<p align="center"><sup>Fallzahlen der Johns-Hopkins-Universität vom 13.11.2020 mit R₀-Werten des Robert Koch-Instituts</sup></p>

Die Basisreproduktionszahl hat sich durch den deutschlandweiten Lockdown und Kontaktverbote deutlich reduziert. Der Peek Mitte Juni entstand wahrscheinlich durch die hohe Anzahl an Corona-Infizierten im Kreis Gütersloh verursacht durch Fleischereibetriebe. Da es sich um einen lokalen Ausbruch handelte, konnte dieser recht schnell eingedämmt werden.

Wenn die historischen R₀-Werte nach der Levenberg-Marquardt-Methode<sup>[10](#f10)</sup> berechnet werden, zeigt das SEIR-Modell die gleichen Werte. Die Fallzahlen (gelbe Linie) entsprechen den tatsächlichen Infektionszahlen (rote Linie) bis zum 13.11.2020, dem aktuellen Datum. Wenn der aktuelle R₀-Wert in Höhe von 1,2 beibehalten wird, ist für die Zukunft jedoch ein weiterer starker Anstieg zu befürchten und eine Verdoppelung der Fallzahlen findet in weniger als einem Monat statt (gelbe Raute).

<p align="center"><img src="images/SEIR%20Germany%20with%20R0.svg" alt="SEIR Prognose mit berechneten R₀ Basisreproduktionszahlen"></p>
<p align="center"><sup>SEIR Prognose mit berechneten R₀ Basisreproduktionszahlen</sup></p>

Der berechnete R₀-Werte im März ist deutlich höher als der vom RKI ermittelte Wert. Dies liegt an den geringen Fallzahlen im Februar und Anfang März. Es ist eine grundsätzliche Schwäche des R₀-Wertes, bei geringen Fallzahlen hohe Schwankungen aufzuweisen. Daher wurde auch im Sommer auf die 7-Tage Inzidenzzahl als Beurteilungskriterium umgestellt.

<p align="center"><img src="images/SEIR%20Germany%207Day%20Incidence.svg" alt="SEIR 7-Tage Inzidenz Prognose mit berechneten R₀ Basisreproduktionszahlen"></p>
<p align="center"><sup>SEIR 7-Tage Inzidenz Prognose mit berechneten R₀ Basisreproduktionszahlen</sup></p>

Auch hier entspricht die Kurve der Johns-Hopkins-Daten dem SEIR Modell. Hier wurde jedoch für die Zukunft ein R₀-Wert von 0.9 verwendet, der nahe bei dem 7-Tages R₀-Wert vom 8.11.2020 des RKI aus der Nowcasting-Tabelle vom 13.11.2020 liegt. In diesem Fall würde die 7-Tages Inzidenz Anfang Dezember unter 100 Fälle pro Tag fallen.

Die unterschiedlichen Szenarien zeigen, wie kritisch die Basisreproduktionszahl für die Weiterentwicklung der Pandemie ist. Ein R₀-Wert über 1 ist der Treiber der Epidemie.

Seit dem 27.12.2020 wird in Deutschland mit dem Biontech Impfstoff geimpft. Die Workbench verwendet ein vereinfachtes Modell, bei dem einfach der Mittelwert der Impfungen pro Tag verwendet wird:

<p align="center"><img src="images/SEIRV%20Germany%20r1,1.svg" alt="SEIRV mit Impfungen ab dem 27.12.2020 und einem angenommenen R₀-Wert von 1,1"></p>
<p align="center"><sup>SEIRV mit Impfungen ab dem 27.12.2020 und einem angenommenen R₀-Wert von 1,1</sup></p>

Stand 17.1.2021 beträgt die mittlere, tägliche Anzahl an Impfungen pro Tag ca. 52.000 Personen in Deutschland. Bei einem geschätzten R₀-Wert von 1,1 würde das Impfprogramm zu fallenden Inzidenzzahlen ab Mitte April beitragen und bis Ende Juni die täglichen Infizierten auf ca. 12.000 pro Tag reduzieren: Dies entspricht einer Inzidenzzahl von ca. 100 Infizierten auf 100.000 Personen in 7 Tagen. Interessanter weise tritt ein positiver Effekt der Impfungen weit vor der bisher vermuteten Schwelle von 60%-70% der Bevölkerung für die Herdenimmunität ein, wie in der Presse bisher veröffentlicht.<sup>[11](#11)</sup> So kompensiert das Impfprogramm bereits ab einer Zahl von 4-5 Mio. Personen einen R₀-Wert von 1,1.

Der entscheidende Faktor innerhalb des Impfprogramms ist die Zahl der täglich geimpften Personen, die einen erheblichen Einfluss auf die Wirksamkeit in der Pandemie hat. Der aktuelle Mittelwert von 52.000 Personen ist sicherlich der im Moment nur geringen Verfügbarkeit des Impfstoffs und der aufwendigen Organisation der Impfungen in Alten- und Pflegeheimen geschuldet, die aufgrund ihrer Vulnerabilität als erste geimpft werden. Trotzdem steigen die Impfzahlen jeden Tag und haben am Freitag, den 14.1.2021 ein Maximum von ca. 98.000 erreicht. Daher ist von ein viel höheren Impfrate in den nächsten Wochen auszugehen. Wenn die mittlere Impfrate z.B. bei 400.000 Impfungen pro Tag liegt, so ist der Effekt auf die Pandemie deutlich stärker:

<p align="center"><img src="images/SEIRV%20Germany%20Max%20R1,3+400.000%20Impfungen.svg" alt="SEIRV mit 400.000 Impfungen pro Tag und einem angenommenen R₀-Wert von 1,3"></p>
<p align="center"><sup>SEIRV mit 400.000 Impfungen pro Tag und einem angenommenen R₀-Wert von 1,3</sup></p>

In diesem Fall wir trotz des höheren R₀-Wertes von 1,3 eine Inzidenzzahl von 12.000 Infizierten pro Tag bereits bis Mitte März erreicht und Ende April fällt die 7-Tages Inzidenz für 100.000 Einwohner auf einen Wert unter 10.

Alles in allem stellen Impfprogramme einen wesentlichen Faktor in der Kontrolle der Corona-Pandemie dar. Ausschlaggebend ist hier eine Impfung möglichst vieler Personen in möglichst kurzer Zeit. Ein Impfprogramm wirkt sich additiv zu anderen, nicht-medizinischen Maßnahmen aus, die den R₀-Wert reduzieren. Die Wirkung werden bereits bei einer Impfabdeckung von 5% der Bevölkerung sichtbar. Diese wird jedoch bei einer mittleren Impfanzahl von ca. 52.000 Personen erst Mitte März erreicht.

Die modellierten Infektionszahlen in dem verwendeten SEIRV-Modell sind jedoch noch sehr vereinfacht und Impfungen könnten präziser berücksichtigt werden. In jedem Fall sollte der Versatz von 12 Tagen zwischen Impfung und erster Immunisierung bzw. 28 Tage bis zur vollständigen Immunisierung bei der Interpretation der Infektionszahlen berücksichtigt werden. Darüber hinaus unterscheiden sich die Wirksamkeiten verschiedener Impfstoffklassen. Die hohe Wirksamkeit der mRNA-Impfstoffe wird von Vektor-basierten Impfstoffen wahrscheinlich nicht erreicht.

### Das Klassenmodell der Workbench
Die Corona Workbench arbeitet mit einem Interface-basierten Klassenmodell für die Berechnung von SEIR-Modellen und SERIV-Modellen. Jedes SEIR-Modell verfügt über eine *ISEIR*-Schnittstelle, in der die SEIR-Parameter angegeben werden und die Personenanzahl für die Kompartimente *S*, *E*, *I* und *R* abgefragt werden können. Mit einer Calc-Methode werden diese für eine bestimmte Anzahl von Zeiteinheiten berechnet. Die Berechnung der Kompartimente *S‘*, *E‘*, *I‘* und *R‘* in der Calc-Methode erfolgt über statische Funktionen und kann hier einfach angepasst werden. Die Initialisierung der *ISEIR*-Modelle findet über Konstruktoren statt.

<p align="center"><img src="images/SEIRV%20Klassenmodell.svg" alt="SEIR & SERIV Klassenmodell"></p>
<p align="center"><sup>SEIR & SERIV Klassenmodell</sup></p>

Um ein *ISEIR*-Objekt anzuzeigen wird dieses an eine Sicht übergeben. Dies kann entweder eine *ISeriesView* für diskrete Zeitabschnitte oder eine *IDateSeriesView* für ein Zeitintervall mit Start- und Enddatum sein. Über die *CalcAsync*-Methoden wird das Modell berechnet und die einzelnen Datenpunkte in Datenserien gespeichert. Die Sicht *SEIRR0DateSeriesView* erlaubt die Übergabe verschiedener R₀-Werte für ein Datum.

Die Berechnung der R₀-Werte erfolgt über Solver mit einer *IR0Solver*-Schnittstelle. Diese implementiert eine *Solve*-Methode, die eine Sequenz von R₀-Werten zurückgibt. Das Interface wird von einer *SEIRR0Solver*-Klasse implementiert, die für jeden Tag die Fallzahlen eines SEIR-Modells mit den tatsächlichen Fallzahlen vergleicht und den besten R₀-Wert für den Tag mit der Levenberg-Marquardt-Methode<sup>[10](#f10)</sup> berechnet. Dafür werden der Einfachheit halber aller R₀-Werte zwischen 0 und 10 in 0,1 Schritten berechnet und der Wert mit der kleinsten Abweichung in den Fallzahlen zurückgegeben.

<p align="center"><img src="images/R0-Solver%20Klassenmodell.svg" alt="Klassenmodell der R₀-Solver"></p>
<p align="center"><sup>Klassenmodell der R₀-Solver</sup></p>

Historische Infektionszahlen des [Center for Systems Science and Engineering](https://systems.jhu.edu/) der Johns-Hopkins werden über ein *JHU*-Objekt ermittelt und als Datenserien über die Sicht *JHUDateSeiriesView* zurückgegeben. Da es sich bei den Daten der Johns-Hopkins-Universität um einen Spezialfall handelt, wurde kein Interface für die *JHU*-Klasse verwendet:

<p align="center"><img src="images/JHU%20Klassenmodell.svg" alt="Klassen für den Abruf der Johns-Hopkins-University CSSE Daten"></p>
<p align="center"><sup>Klassen für den Abruf der Johns-Hopkins-University CSSE Daten</sup></p>

Das *JHU*-Objekt speichert die abgerufenen Daten in einer temporären CSV-Datei zwischen und versucht diese möglichst schnell zu parsen.
Den gleichen Ansatz verfolgt die *RKINowcasting*-Klasse, die die [Nowcasting-Excel-Liste](https://www.rki.de/DE/Content/InfAZ/N/Neuartiges_Coronavirus/Projekte_RKI/Nowcasting_Zahlen.xlsx) des [Robert Koch-Instituts](https://www.rki.de/) einmal täglich herunterlädt und die Spalten „Punktschätzer der Reproduktionszahl R“, sowie „Punktschätzer des 7-Tage-R Wertes“ für ein Datum zurückgibt.

<p align="center"><img src="images/RKI%20Nowcasting%20Klassenmodell.svg" alt="Klassen für den Abruf der Robert Koch-Institut Nowcasting-Daten"></p>
<p align="center"><sup>Klassen für den Abruf der Robert Koch-Institut Nowcasting-Daten</sup></p>

Daten über geimpfte Personen je Land werden derzeit von [Our World in Data](https://ourworldindata.org/covid-vaccinations) der Oxford Martin School und University of Oxford ermittelt und über [GitHub](https://github.com/owid/covid-19-data/tree/master/public/data/vaccinations) zur Verfügung gestellt.

<p align="center"><img src="images/OWID%20Klassenmodell.svg" alt="Klassen für den Abruf der OWID-Daten über Impfungen"></p>
<p align="center"><sup>Klassen für den Abruf der OWID-Daten über Impfungen</sup></p>

Die aktuelle Bevölkerungszahl eines Landes wird mit der Klasse *WPPopulation* über das [World-Bank API](https://data.worldbank.org/) ermittelt. 

## Ausblick
Die Corona SEIR Workbench liefert eine einfache Infrastruktur zur Modellierung von Epidemien. Sie verwendet Standardkomponenten, wie das Windows Forms Charting für die Anzeige von Diagrammen und die Windows Presentation Foundation für die Benutzeroberfläche. Aktuelle epidemiologische Daten für COVID 19 können komponentenweise aus dem Web geladen werden und mit Modellwerten verglichen werden.

Dadurch kann die Anwendung mit wenig Aufwand erweitert werden. Erweiterungen wären z.B. eine zusätzliche Datenbasis mit den Infektionsdaten des RKI und einer regionalen Aufteilung für Deutschland. Darüber hinaus könnten zukünftige R₀-Werte nicht nur als einzelne Konstante, sondern als Funktion oder Wertetabelle angegeben werden.

Die Berechnung des R₀-Wertes ist maßgeblich von der Testsituation abhängig. So verfälscht eine hohe Dunkelziffer von Infizierten die Berechnung des R₀-Wertes aus historischen Infektionszahlen. Da besonders junge Infizierte asymptomatisch sein können, ist die Altersverteilung der Infizierten bei der Abschätzung der Dunkelziffer relevant. Darüber hinaus existieren Rückkopplungsschleifen mit Epidemie-Maßnahmen, wie z.B. bei der Quarantäne von Reiserückkehrern aus Risikogebieten die wahrscheinlich häufiger trotz Symptomen ohne Testung von infizierten Personen unterlaufen wurde.

Mit dem Start von Impfprogrammen wurde das SEIR-Modell um ein Kompartiment für geimpft Personen erweitert. Da Impfungen eine erhebliche Auswirkung auf die weitere Pandemieentwicklung haben, liefert das erweiterte SEIRV-Modell genauere Voraussagen. Im Gegensatz zur genauso möglichen Berücksichtigung der Impfungen in der Basisreproduktionszahl R₀ erlaubt die Verwendung eines gesonderten Kompartimentes eine klarere Trennung zwischen der Wirkung von Impfungen und nicht-medizinischen Maßnahmen. 

Im weiteren Verlauf der Pandemie muss jedoch berücksichtigt werden, dass die R₀ – Werte des RKI für Deutschland geringer ausfallen wie die der Corona SEIR Workbench, da diese aktuell den Rückgang von Infizierten durch Impfungen beinhalten und nicht wie das SEIRV-Modell diese vorher herausrechnen.

Eine interessante Erweiterung des SEIR-Modells ist die Berücksichtigung von [Superspreading-Ereignissen](https://de.wikipedia.org/wiki/Superspreading) und [Perkolation](https://de.wikipedia.org/wiki/Perkolationstheorie), die sich durch die [Überdispersion](https://de.wikipedia.org/wiki/%C3%9Cberdispersion) von Corona-Infektionen ergibt. Zur Nachverfolgung von Superspreading-Ereignissen bietet sich ein [Cluster-Tagebuch](https://www.logiclink.de/2020/10/22/superspreading-covid-19-pandemie/) an.

Für die Berechnung des Aufwandes einer medizinischen Versorgung ist die Aufteilung der infizierten Personen in leichte Fälle und schwere Fälle notwendig. Hierfür sind jedoch Daten zur Altersstruktur der Infizierten notwendig.

Eine viel weitergehende Modellierung wären z.B. die Ansätze von Frau Prof. Priesemann zur altersabhängige Todesrate von Infizierten (infection fatality rate, IFR)<sup>[12](#f12)</sup> oder die Auswirkungen einer Test-, Nachverfolgungs- und Isolierungsstrategie (TTI, Test-Trace-Isolate)<sup>[13](#f13)</sup>.


*__Letztendlich ist die Corona SEIR Workbench nur eine Infrastruktur und wartet auf den nächsten Einsatz. Wir sind gespannt, welche Einsatzmöglichkeiten Sie finden.__*

Fragen oder Anregungen zur Workbench können Sie uns entweder direkt [zumailen](mailto://info@logiclink.de) oder als [Eintrag](https://github.com/logiclink/corona_seir_workbench/issues) bei Github schreiben.

*[LogicLink, Marcus Müller](https://www.logiclink.de), Januar 2021*

<a name="f1">1.</a> [Interview mit Gunther Kraut, Handelsblatt vom 02.05.2020](https://www.handelsblatt.com/finanzen/banken-versicherungen/gunther-kraut-pandemie-experte-der-munich-re-alle-20-bis-30-jahre-kann-so-etwas-wie-corona-passieren/25770456.html) <br/>
<a name="f2">2.</a> [Li Q, Guan X, Wu P, Wang X, Zhou L, Tong Y, Ren R, Leung KSM, Lau EHY, Wong JY, Xing X, Xiang N, Wu Y, Li C, Chen Q, Li D, Liu T, Zhao J, Liu M, Tu W, Chen C, Jin L, Yang R, Wang Q, Zhou S, Wang R, Liu H, Luo Y, Liu Y, Shao G, Li H, Tao Z, Yang Y, Deng Z, Liu B, Ma Z, Zhang Y, Shi G, Lam TTY, Wu JT, Gao GF, Cowling BJ, Yang B, Leung GM, Feng Z. Early Transmission Dynamics in Wuhan, China, of Novel Coronavirus-Infected Pneumonia. N Engl J Med. 2020 Mar 26;382(13):1199-1207. doi: 10.1056/NEJMoa2001316. Epub 2020 Jan 29. PMID: 31995857; PMCID: PMC7121484.](https://www.nejm.org/doi/full/10.1056/NEJMoa2001316) <br/>
<a name="f3">3.</a> [Byrne AW, McEvoy D, Collins AB, et al., Inferred duration of infectious period of SARS-CoV-2: rapid scoping review and analysis of available evidence for asymptomatic and symptomatic COVID-19 cases, BMJ Open 2020;10:e039856. doi:10.1136/bmjopen-2020-039856](https://bmjopen.bmj.com/content/10/8/e039856) <br/>
<a name="f4">4.</a> [Wu, Jianhong & Leung, Kathy & Leung, Gabriel. (2020). Nowcasting and forecasting the potential domestic and international spread of the 2019-nCoV outbreak originating in Wuhan, China: a modelling study. The Lancet. 395. 10.1016/S0140-6736(20)30260-9.](https://www.thelancet.com/action/showPdf?pii=S0140-6736%2820%2930260-9)  <br/>
<a name="f5">5.</a> [Safarishahrbijari A, Lawrence T, Lomotey R, Liu J, Waldner C, Osgood N. Particle filtering in a SEIRV simulation model of H1N1 influenza. In: Winter Simulation Conference (WSC), 2015. IEEE; 2015. p. 1240–1251.](https://www.researchgate.net/profile/Anahita_Safarishahrbijari/publication/302480311_Particle_filtering_in_a_SEIRV_simulation_model_of_H1N1_influenza/links/5ecc2fcea6fdcc90d6998e8d/Particle-filtering-in-a-SEIRV-simulation-model-of-H1N1-influenza.pdf) <br/>
<a name="f6">6.</a> [Polack F. et al., 2020, Safety and Efficacy of the BNT162b2 mRNA Covid-19 Vaccine. N Engl J Med 383:27, 2603-2615.](https://www.nejm.org/doi/full/10.1056/NEJMoa2034577)  <br/>
<a name="f7">7.</a> [Wu, Joseph T et al., Nowcasting and forecasting the potential domestic and international spread of the 2019-nCoV outbreak originating in Wuhan, China: a modelling study, The Lancet, 2020 Volume 395, Issue 10225, 689 - 697](https://www.thelancet.com/journals/lancet/article/PIIS0140-6736(20)30260-9/fulltext)  <br/>
<a name="f8">8.</a> Beispiele unter https://github.com/dotnet/winforms-datavisualization/tree/main/sample  <br/>
<a name="f9">9.</a> Pina Merkert, ODE an Corona – Covid-19-Vorhersagen mit dem SEIR Modell, c’t 2020, Heft 11, Seite 124-127  <br/>
<a name="f10">10.</a> [Craig Markwardt, Least Squares Fi-ng and Equaton Solving with MPFIT, University of Maryland and NASA’s Goddard Spaceflight Center, http://purl.com/net/mpfit 2009‐04-15](http://cow.physics.wisc.edu/~craigm/idl/Markwardt-MPFIT-Visualize2009.pdf)  <br/>
<a name="f11">11.</a> [Durchimpfungsrate - Wie viel Prozent der Bevölkerung muss geimpft werden, um ein "normales Leben" zu ermöglichen? - Nachrichten - WDR](https://www1.wdr.de/nachrichten/themen/coronavirus/corona-impfung-faq-durchimpfung-100.html)  <br/>
<a name="f12">12.</a> [Linden, Matthias, u. a. „The foreshadow of a second wave: An analysis of current COVID-19 fatalities in Germany". arXiv:2010.05850 [physics, q-bio], Oktober 2020. arXiv.org, http://arxiv.org/abs/2010.05850.](https://arxiv.org/pdf/2010.05850v2.pdf)  <br/>
<a name="f13">13.</a> [Contreras, Sebastian, u. a. „The challenges of containing SARS-CoV-2 via test-trace-and-isolate". arXiv:2009.05732 [q-bio], November 2020. arXiv.org, http://arxiv.org/abs/2009.05732.](https://arxiv.org/pdf/2009.05732v2.pdf)
