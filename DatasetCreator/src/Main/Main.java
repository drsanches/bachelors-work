package Main;

import org.scilab.forge.jlatexmath.*;
import java.awt.*;
import java.io.File;
import java.util.Scanner;


public class Main {

    public static void main(String[] args) {
        final String SYMBOLS_FILENAME = "dataset\\symbols.txt";
        final String DIRECTORY = "dataset\\images";
        final String OUTPUT_FILENAME = "dataset\\dataset.npz";
        final String SCRIPT_FILENAME = "dataset\\images_to_dataset.py";

        try (Scanner scanner = new Scanner(new File(SYMBOLS_FILENAME))) {
            int count = 0;
            while (scanner.hasNext()) {
                String symbol = scanner.nextLine();
                if (!symbol.equals("")) {
                    createSymbolImage(symbol, DIRECTORY + "\\" + count + ".png");
                    System.out.println(symbol);
                    count++;
                }
            }
            scanner.close();
            imagesToDataset(SCRIPT_FILENAME, SYMBOLS_FILENAME, DIRECTORY, OUTPUT_FILENAME);
        } catch (Exception e) {
            System.out.println("Error: " + e.getMessage());
        }

        System.out.println("Done");
    }

    private static void createSymbolImage(String symbol, String filename) {
        TeXFormula teXFormula = new TeXFormula(symbol);
        teXFormula.createPNG(TeXConstants.STYLE_DISPLAY, 50, filename, Color.white, Color.black);
    }

    private static void imagesToDataset(String scriptFilename, String filename,
                                        String directory, String outputFilename) throws Exception {
        Process process = new ProcessBuilder()
                .command("python.exe", scriptFilename, filename, directory, outputFilename)
                .start();
        System.out.println(process.waitFor());
    }
}
