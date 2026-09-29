// Decompile the functions containing the given addresses (creates the function if missing).
// Usage (headless, -process P3P_noarch.exe -noanalysis -readOnly):
//   -postScript Decomp.java <outfile> 0x14014B350 0x...
import ghidra.app.script.GhidraScript;
import ghidra.app.decompiler.*;
import ghidra.program.model.address.Address;
import ghidra.program.model.listing.Function;
import java.io.*;

public class Decomp extends GhidraScript {
    @Override
    public void run() throws Exception {
        String[] args = getScriptArgs();
        PrintWriter out = new PrintWriter(new OutputStreamWriter(new FileOutputStream(args[0], true), "UTF-8"));
        DecompInterface di = new DecompInterface();
        di.openProgram(currentProgram);
        for (int i = 1; i < args.length; i++) {
            Address a = toAddr(Long.decode(args[i]));
            Function f = getFunctionContaining(a);
            if (f == null) {
                disassemble(a);
                f = createFunction(a, null);
            }
            if (f == null) { out.println("// no function at " + a); continue; }
            DecompileResults r = di.decompileFunction(f, 120, monitor);
            out.println("// ===== " + f.getName() + " @ " + f.getEntryPoint() + " (asked " + a + ")");
            out.println(r.decompileCompleted() ? r.getDecompiledFunction().getC() : "// failed: " + r.getErrorMessage());
        }
        out.close();
    }
}
