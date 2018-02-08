package Main;

import org.scilab.forge.jlatexmath.*;
import java.awt.*;
import java.io.BufferedReader;
import java.io.FileInputStream;
import java.io.InputStreamReader;
import org.json.*;


public class Main {

    public static void main(String[] args) {
        final String SYMBOLS_FILENAME = "..\\dataset\\symbols.json";
        final String IMAGES_DIRECTORY = "..\\dataset\\images";
        final String OUTPUT_FILENAME = "..\\dataset\\dataset.npz";
        final String SCRIPT_FILENAME = "images_to_dataset.py";


        try {
            JSONObject jsonSymbols = readJsonFromFile(SYMBOLS_FILENAME);
            String[] symbols = jsonSymbols.getString("Symbols").split(" ");

            for (int i = 0; i < symbols.length; i++) {
                createSymbolImage(symbols[i], IMAGES_DIRECTORY + "\\" + i + ".png");
                System.out.println(symbols[i]);
            }

            imagesToDataset(SCRIPT_FILENAME, SYMBOLS_FILENAME, IMAGES_DIRECTORY, OUTPUT_FILENAME);

        } catch (Exception e) {
            System.out.println("Error: " + e.getMessage());
        }

        System.out.println("Done");
    }

    private static JSONObject readJsonFromFile(String filename) throws Exception {
        String fileContent = "";
        BufferedReader reader = new BufferedReader(new InputStreamReader(new FileInputStream(filename)));
        String nextString;
        while ((nextString = reader.readLine()) != null)
            fileContent += nextString;
        reader.close();

        return new JSONObject(fileContent);
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
