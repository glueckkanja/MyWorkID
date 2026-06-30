import { useRef, useState } from "react";
import { revokeTemporaryAccessPass } from "../../../services/api-service";
import { useToast } from "@/hooks/use-toast";

const SWIPE_COMPLETE_THRESHOLD = 150;
const SWIPE_INITIATION_THRESHOLD = 8;

type UseRevokeTemporaryAccessPassOptions = {
  temporaryAccessPassId: string;
  isShowingToken: boolean;
  onRevoked: () => void;
};

export function useRevokeTemporaryAccessPass({
  temporaryAccessPassId,
  isShowingToken,
  onRevoked,
}: UseRevokeTemporaryAccessPassOptions) {
  const [revoking, setRevoking] = useState(false);
  const [swipeOffset, setSwipeOffset] = useState(0);
  const [isSwipeGestureActive, setIsSwipeGestureActive] = useState(false);
  const swipeStartXRef = useRef<number | null>(null);
  const swipeOffsetRef = useRef(0);
  const swipeStatus = useRef(false);
  const { toastException, toastError, toastSuccess } = useToast();

  const handleRevokeTemporaryAccessPass = () => {
    setRevoking(true);
    revokeTemporaryAccessPass(temporaryAccessPassId)
      .then((result) => {
        if (result.status === "success") {
          setRevoking(false);
          toastSuccess("Temporary Access Pass revoked.", "");
          onRevoked();
        } else {
          setRevoking(false);
          toastError();
        }
      })
      .catch((error) => {
        toastException(error);
        setRevoking(false);
      });
  };

  const handleSwipePointerDown = (
    event: React.PointerEvent<HTMLDivElement>,
  ) => {
    if (!isShowingToken || revoking) {
      return;
    }
    const target = event.target as HTMLElement;
    if (target.closest(".action-card-revoke__tap")) {
      return;
    }
    swipeStartXRef.current = event.clientX;
    swipeOffsetRef.current = 0;
    setSwipeOffset(0);
    setIsSwipeGestureActive(true);
    (event.currentTarget as HTMLElement).setPointerCapture(event.pointerId);
  };

  const handleSwipePointerMove = (
    event: React.PointerEvent<HTMLDivElement>,
  ) => {
    if (swipeStartXRef.current === null || !isShowingToken || revoking) {
      return;
    }
    const offset = Math.max(0, event.clientX - swipeStartXRef.current);
    swipeOffsetRef.current = offset;
    setSwipeOffset(offset);
  };

  const handleSwipePointerUp = () => {
    if (swipeStartXRef.current === null) {
      return;
    }
    if (swipeOffsetRef.current > SWIPE_INITIATION_THRESHOLD) {
      swipeStatus.current = true;
      if (swipeOffsetRef.current >= SWIPE_COMPLETE_THRESHOLD) {
        handleRevokeTemporaryAccessPass();
      }
    }
    swipeStartXRef.current = null;
    swipeOffsetRef.current = 0;
    setSwipeOffset(0);
    setIsSwipeGestureActive(false);
  };

  const handleSwipePointerCancel = () => {
    swipeStartXRef.current = null;
    swipeOffsetRef.current = 0;
    setSwipeOffset(0);
    setIsSwipeGestureActive(false);
  };

  return {
    revoking,
    isSwipeGestureActive,
    swipeProgress: Math.min(swipeOffset / SWIPE_COMPLETE_THRESHOLD, 1),
    swipeStatus: swipeStatus,
    handleRevokeTemporaryAccessPass,
    handleSwipePointerDown,
    handleSwipePointerMove,
    handleSwipePointerUp,
    handleSwipePointerCancel,
  };
}
