// List references to the given addresses (code and data), with the containing function.
// Usage: -postScript XRefs.java <outfile> 0x14077ee40 ...
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import ghidra.program.model.listing.Function;
import ghidra.program.model.symbol.Reference;
import java.io.*;

public class XRefs extends GhidraScript {
    @Override
    public void run() throws Exception {
        String[] args = getScriptArgs();
        PrintWriter out = new PrintWriter(new OutputStreamWriter(new FileOutputStream(args[0], true), "UTF-8"));
        for (int i = 1; i < args.length; i++) {
            Address a = toAddr(Long.decode(args[i]));
            out.println("== refs to " + a);
            for (Reference r : getReferencesTo(a)) {
                Function f = getFunctionContaining(r.getFromAddress());
                out.println("  " + r.getFromAddress() + " " + r.getReferenceType() + " in " + (f == null ? "?" : f.getName() + "@" + f.getEntryPoint()));
            }
        }
        out.close();
    }
}
